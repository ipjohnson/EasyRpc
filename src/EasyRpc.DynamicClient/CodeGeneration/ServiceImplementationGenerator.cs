using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using EasyRpc.Abstractions.Path;
using EasyRpc.DynamicClient.Serializers;

namespace EasyRpc.DynamicClient.CodeGeneration
{
    public class ImplementationRequest
    {
        public Type InterfaceType { get; set; }

        public string HostKey { get; set; }

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
        private readonly object _lock = new object();
        private readonly ModuleBuilder _moduleBuilder;
        private int _proxyCount = 0;

        public ServiceImplementationGenerator()
        {
            var dynamicAssembly = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName(Guid.NewGuid().ToString()), AssemblyBuilderAccess.Run);

            _moduleBuilder = dynamicAssembly.DefineDynamicModule("ClientServiceImplementation");
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

                return typeBuilder.CreateTypeInfo().AsType();
            }
        }

        private void GenerateMethods(TypeBuilder typeBuilder, ImplementationRequest request,
            GenerationContext generationContext)
        {
            foreach (var methodInfo in request.InterfaceType.GetMethods(BindingFlags.Instance | BindingFlags.Public))
            {
                GenerateMethod(typeBuilder, request, generationContext, methodInfo);
            }
        }

        private void GenerateMethod(TypeBuilder typeBuilder, ImplementationRequest request, GenerationContext generationContext, MethodInfo methodInfo)
        {
            var attributes = methodInfo.GetCustomAttributes<Attribute>(true).ToArray();

            if (attributes.Any(a => a is IgnoreMethodAttribute))
            {
                return;
            }

            var pathAttribute = attributes.FirstOrDefault(a => a is IPathAttribute);

            if (pathAttribute != null)
            {
                GenerateMethodFromPathAttribute(typeBuilder, request, generationContext, methodInfo,attributes, pathAttribute);
            }
            else
            {
                GenerateMethodAsDefaultExpose(typeBuilder, request, generationContext, methodInfo, attributes);
            }
        }

        private void GenerateMethodAsDefaultExpose(TypeBuilder typeBuilder, ImplementationRequest request,
            GenerationContext generationContext, MethodInfo methodInfo, Attribute[] attributes)
        {
            
        }

        private void GenerateMethodFromPathAttribute(TypeBuilder typeBuilder, ImplementationRequest request,
            GenerationContext generationContext, MethodInfo methodInfo, Attribute[] attributes, Attribute pathAttribute)
        {
            
        }

        private void GenerateConstructor(TypeBuilder typeBuilder, ImplementationRequest request,
            GenerationContext generationContext)
        {
            
        }

        protected virtual TypeBuilder CreateTypeBuilder(ImplementationRequest request,
            GenerationContext generationContext)
        {
            return _moduleBuilder.DefineType("ClientInterfaceProxy" + _proxyCount, TypeAttributes.Public);
        }

        public class GenerationContext
        {
            public FieldBuilder RpcExecutionServiceField { get; set; }
        }
    }
}
