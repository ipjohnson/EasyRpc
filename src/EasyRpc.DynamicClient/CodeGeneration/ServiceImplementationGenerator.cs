using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using EasyRpc.Abstractions.Path;
using EasyRpc.DynamicClient.ExecutionService;
using EasyRpc.DynamicClient.Serializers;
using CancellationToken = System.Threading.CancellationToken;

namespace EasyRpc.DynamicClient.CodeGeneration
{
    public class ImplementationRequest
    {
        public Type InterfaceType { get; set; }

        public string BasePath { get; set; }

        public IRpcHttpClientProvider ClientProvider { get; set; }

        public ExposeDefaultMethod ExposeDefaultMethod { get; set; }

        public IClientSerializer DefaultSerializer { get; set; }

        public bool SingleParameterToBody { get; set; }
    }

    public interface IServiceImplementationGenerator
    {
        Type GenerateImplementationForInterface(ImplementationRequest request);
    }

    public class ServiceImplementationGenerator : IServiceImplementationGenerator
    {
        private readonly MethodInfo _rpcExecTaskMethodInfo;
        private readonly MethodInfo _rpcExecTaskWithValueMethodInfo;
        private readonly MethodInfo _cancellationNone;
        private readonly ConstructorInfo _cancellationConstructor;
        private readonly ISerializationTypeCreator _serializationTypeCreator;
        private readonly object _lock = new object();
        private readonly ModuleBuilder _moduleBuilder;
        private int _proxyCount = 0;

        public ServiceImplementationGenerator(ISerializationTypeCreator serializationTypeCreator)
        {
            _serializationTypeCreator = serializationTypeCreator;
            var dynamicAssembly = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName(Guid.NewGuid().ToString()), AssemblyBuilderAccess.Run);

            _moduleBuilder = dynamicAssembly.DefineDynamicModule("ClientServiceImplementation");
            
            _rpcExecTaskMethodInfo = typeof(IRpcExecutionService).GetMethod(nameof(IRpcExecutionService.ExecuteMethod));
            _rpcExecTaskWithValueMethodInfo = typeof(IRpcExecutionService).GetMethod(nameof(IRpcExecutionService.ExecuteMethodWithValue));
            _cancellationNone = typeof(CancellationToken).GetProperty("None").GetGetMethod();
            _cancellationConstructor = typeof(CancellationToken?).GetConstructors().First();
        }

        public Type GenerateImplementationForInterface(ImplementationRequest request)
        {
            lock (_lock)
            {
                _proxyCount++;

                var generationContext = new GenerationContext();

                var typeBuilder = CreateTypeBuilder(request, generationContext);

                GenerateConstructor(typeBuilder, request, generationContext);

                GenerateMethods(typeBuilder, request, generationContext);

                var type = typeBuilder.CreateTypeInfo().AsType();

                foreach (var finalizeTypeAction in generationContext.FinalizeTypeActions)
                {
                    finalizeTypeAction(type);
                }
                
                return type;
            }
        }

        private void GenerateMethods(TypeBuilder typeBuilder, ImplementationRequest request,
            GenerationContext generationContext)
        {
            foreach (var methodInfo in request.InterfaceType.GetMethods(BindingFlags.Instance | BindingFlags.Public))
            {
                var methodGenerationContext = new MethodGenerationContext(generationContext);

                GenerateMethod(typeBuilder, request, methodGenerationContext, methodInfo);
            }
        }

        private void GenerateMethod(TypeBuilder typeBuilder, ImplementationRequest request, MethodGenerationContext generationContext, MethodInfo methodInfo)
        {
            var attributes = methodInfo.GetCustomAttributes<Attribute>(true).ToArray();

            if (attributes.Any(a => a is IgnoreMethodAttribute))
            {
                return;
            }

            var pathAttribute = attributes.FirstOrDefault(a => a is IPathAttribute);

            if (pathAttribute != null)
            {
                GenerateMethodFromPathAttribute(typeBuilder, request, generationContext, methodInfo, attributes, pathAttribute);
            }
            else
            {
                GenerateMethodAsDefaultExpose(typeBuilder, request, generationContext, methodInfo, attributes);
            }
        }

        private void GenerateMethodAsDefaultExpose(TypeBuilder typeBuilder, ImplementationRequest request,
            MethodGenerationContext generationContext, MethodInfo methodInfo, Attribute[] attributes)
        {
            PopulateGenerationContext(typeBuilder, request, generationContext, methodInfo, attributes);

            GenerateMethodImplementation(typeBuilder, request, generationContext, methodInfo, attributes);
        }


        private void PopulateGenerationContext(TypeBuilder typeBuilder, ImplementationRequest request, MethodGenerationContext generationContext, MethodInfo methodInfo, Attribute[] attributes)
        {
            HttpMethod method = HttpMethod.Post;

            if (request.ExposeDefaultMethod == ExposeDefaultMethod.PostOnly)
            {
                generationContext.PathTemplate = request.BasePath + methodInfo.DeclaringType?.Name + "/" + methodInfo.Name;

                generationContext.BodyParameters = new List<ParameterInfo>();

                generationContext.BodyParameters.AddRange(methodInfo.GetParameters());
            }
            else
            {
                throw new NotImplementedException();
            }

            GenerateRpcExecuteInformation(typeBuilder, request, generationContext, methodInfo, attributes, method);
        }

        private void GenerateMethodImplementation(TypeBuilder typeBuilder, ImplementationRequest request, MethodGenerationContext generationContext, MethodInfo methodInfo, Attribute[] attributes)
        {
            var parameters = methodInfo.GetParameters();

            var methodAttributes = MethodAttributes.Public |
                                   MethodAttributes.HideBySig |
                                   MethodAttributes.Virtual |
                                   MethodAttributes.NewSlot;

            var methodBuilder = typeBuilder.DefineMethod(methodInfo.Name, methodAttributes, CallingConventions.Standard,
                methodInfo.ReturnType, parameters.Select(p => p.ParameterType).ToArray());


            GenerateMethodImplementationIL(typeBuilder, request, generationContext, methodInfo, attributes, methodBuilder);
        }

        private void GenerateMethodImplementationIL(TypeBuilder typeBuilder, ImplementationRequest request,
            MethodGenerationContext generationContext, MethodInfo methodInfo, Attribute[] attributes,
            MethodBuilder methodBuilder)
        {
            var il = methodBuilder.GetILGenerator();

            if (generationContext.UrlParameters != null &&
                generationContext.UrlParameters.Count > 0)
            {
                throw new NotImplementedException();
            }

            if (generationContext.BodyParameters != null &&
                generationContext.BodyParameters.Count > 0)
            {
                GenerateBodyVariable(typeBuilder, request, il, generationContext, methodInfo);
            }

            GenerateRpcExecutionMethodCall(generationContext, methodInfo, il);

            il.Emit(OpCodes.Ret);
        }

        private void GenerateBodyVariable(TypeBuilder typeBuilder, ImplementationRequest request, ILGenerator il,
            MethodGenerationContext generationContext,
            MethodInfo methodInfo)
        {
            // single parameter is loaded directly from the parameter no need for a local variable
            if (request.SingleParameterToBody &&
                generationContext.BodyParameters.Count == 1)
            {
                return;
            }

            var serializedType =
                _serializationTypeCreator.CreateSerializationTypeForMethod(generationContext.BodyParameters);

            var bodyVariable = il.DeclareLocal(serializedType);

            generationContext.BodyVariable = bodyVariable;
            var constructor = serializedType.GetConstructor(new Type[0]);

            il.Emit(OpCodes.Newobj, constructor);
            il.Emit(OpCodes.Stloc, generationContext.BodyVariable);
            
            foreach (var bodyParameter in generationContext.BodyParameters)
            {
                var propertyInfo = serializedType.GetProperty(bodyParameter.Name);

                var setMethod = propertyInfo?.GetSetMethod();

                if (setMethod == null)
                {
                    // should never get here as the type is auto generated
                    throw new Exception($"Could not find set method on serialized type {serializedType.Name} property {bodyParameter.Name}");
                }

                il.Emit(OpCodes.Ldloc, bodyVariable);
                il.EmitLoadArg(bodyParameter.Position + 1);
                il.EmitMethodCall(setMethod);
            }
        }

        private void GenerateRpcExecutionMethodCall(MethodGenerationContext generationContext, MethodInfo methodInfo,
            ILGenerator il)
        {
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldfld, generationContext.GenerationContext.RpcExecutionServiceField);

            il.Emit(OpCodes.Ldsfld, generationContext.ExecutionInfoField);

            if (generationContext.PathVariable != null)
            {
                il.Emit(OpCodes.Ldloc, generationContext.PathVariable);
            }
            else
            {
                il.Emit(OpCodes.Ldstr, generationContext.PathTemplate);
            }
            
            if (generationContext.BodyVariable != null)
            {
                il.Emit(OpCodes.Ldloc, generationContext.BodyVariable);
            }
            else if (generationContext.BodyParameters?.Count == 1)
            {
                il.EmitLoadArg(generationContext.BodyParameters[0].Position);
            }
            else
            {
                il.Emit(OpCodes.Ldnull);
            }
            
            if (generationContext.CancellationTokenParameter != null)
            {
                il.EmitLoadArg(generationContext.CancellationTokenParameter.Position);
            }
            else
            {
                il.Emit(OpCodes.Call, _cancellationNone);
                il.Emit(OpCodes.Newobj, _cancellationConstructor);
            }

            if (methodInfo.ReturnType == typeof(Task))
            {
                il.EmitMethodCall(_rpcExecTaskMethodInfo);
            }
            else if (methodInfo.ReturnType.IsConstructedGenericType && methodInfo.ReturnType.GetGenericTypeDefinition() == typeof(Task<>))
            {
                var argumentType = methodInfo.ReturnType.GenericTypeArguments[0];

                var closedMethod = _rpcExecTaskWithValueMethodInfo.MakeGenericMethod(argumentType);

                il.EmitMethodCall(closedMethod);
            }
            else
            {
                throw new NotImplementedException();
            }
        }


        private void GenerateRpcExecuteInformation(TypeBuilder typeBuilder, ImplementationRequest request,
            MethodGenerationContext generationContext, MethodInfo methodInfo, Attribute[] attributes, HttpMethod method)
        {
            var fieldName = "EasyRpcInfo_" + methodInfo.Name;

            generationContext.ExecutionInfoField =
                typeBuilder.DefineField(fieldName, typeof(RpcExecuteInformation), FieldAttributes.Static | FieldAttributes.Public);

            generationContext.ExecuteInformation = new RpcExecuteInformation
            {
                Method = method,
                ClientProvider = request.ClientProvider,
                Serializer = request.DefaultSerializer
            };

            generationContext.GenerationContext.FinalizeTypeActions.Add(t =>
                {
                    var field = t.GetField(fieldName, BindingFlags.Static | BindingFlags.Public);

                    if (field == null)
                    {
                        throw new Exception($"Could not locate static field {fieldName} on {t.Name} for interface {methodInfo.DeclaringType?.Name}.{methodInfo.Name}");
                    }

                    field.SetValue(null, generationContext.ExecuteInformation);
                });
        }

        private void GenerateConstructor(TypeBuilder typeBuilder, ImplementationRequest request,
            GenerationContext generationContext)
        {
            generationContext.RpcExecutionServiceField = typeBuilder.DefineField("_rpcExecutionService", typeof(IRpcExecutionService), FieldAttributes.Private);

            var constructor = typeBuilder.DefineConstructor(MethodAttributes.Public, CallingConventions.Standard,
                new[] { typeof(IRpcExecutionService) });

            constructor.DefineParameter(1, ParameterAttributes.None, "rpcExecutionService");

            GenerateConstructorIL(typeBuilder, request, generationContext, constructor);
        }

        private void GenerateConstructorIL(TypeBuilder typeBuilder, ImplementationRequest request, GenerationContext generationContext, ConstructorBuilder constructor)
        {
            var ilGenerator = constructor.GetILGenerator();

            ilGenerator.Emit(OpCodes.Ldarg_0);
            ilGenerator.Emit(OpCodes.Ldarg_1);
            ilGenerator.Emit(OpCodes.Stfld, generationContext.RpcExecutionServiceField);
            ilGenerator.Emit(OpCodes.Ret);
        }

        protected virtual TypeBuilder CreateTypeBuilder(ImplementationRequest request,
            GenerationContext generationContext)
        {
            var interfaceType = _moduleBuilder.DefineType("ClientInterfaceProxy" + _proxyCount, TypeAttributes.Public);

            interfaceType.AddInterfaceImplementation(request.InterfaceType);

            return interfaceType;
        }

        private void GenerateMethodFromPathAttribute(TypeBuilder typeBuilder, ImplementationRequest request,
            MethodGenerationContext generationContext, MethodInfo methodInfo, Attribute[] attributes, Attribute pathAttribute)
        {

        }


        public class GenerationContext
        {
            public FieldBuilder RpcExecutionServiceField { get; set; }

            public List<Action<Type>> FinalizeTypeActions { get; } = new List<Action<Type>>();
        }

        public class MethodGenerationContext
        {
            public MethodGenerationContext(GenerationContext generationContext)
            {
                GenerationContext = generationContext;
            }

            public FieldBuilder ExecutionInfoField { get; set; }

            public GenerationContext GenerationContext { get; }

            public List<ParameterInfo> UrlParameters { get; set; }

            public List<ParameterInfo> BodyParameters { get; set; }

            public ParameterInfo CancellationTokenParameter { get; set; }

            public RpcExecuteInformation ExecuteInformation { get; set; }

            public LocalBuilder PathVariable { get; set; }

            public LocalBuilder BodyVariable { get; set; }

            public string PathTemplate { get; set; }
        }
    }
}
