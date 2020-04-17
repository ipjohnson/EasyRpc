using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using EasyRpc.AspNetCore.EndPoints;

namespace EasyRpc.AspNetCore.CodeGeneration
{
    public interface IDeserializationTypeCreator
    {
        Type CreateTypeForMethod(EndPointMethodConfiguration methodConfiguration);
    }

    public class DeserializationTypeCreator : IDeserializationTypeCreator
    {
        private readonly object _lock = new object();
        private readonly ModuleBuilder _moduleBuilder;
        private int _proxyCount = 0;
        private readonly MethodInfo _stringEqual = typeof(string).GetMethod("Equals", new[] { typeof(string), typeof(string) });

        public DeserializationTypeCreator()
        {
            var dynamicAssembly = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName(Guid.NewGuid().ToString()), AssemblyBuilderAccess.Run);

            _moduleBuilder = dynamicAssembly.DefineDynamicModule("DeserializeTypes");
        }

        public Type CreateTypeForMethod(EndPointMethodConfiguration methodConfiguration)
        {
            lock (_lock)
            {
                _proxyCount++;

                var creationContext = new TypeCreationContext();

                var typeBuilder = CreateTypeBuilder(methodConfiguration, creationContext);


                AddInterfaceImplementations(typeBuilder, methodConfiguration, creationContext);

                CreateBackingFields(typeBuilder, methodConfiguration, creationContext);

                if (methodConfiguration.Parameters.Any(p => p.HasDefaultValue))
                {
                    GenerateConstructorWithDefaults(typeBuilder, methodConfiguration, creationContext);
                }

                ImplementProperties(typeBuilder, methodConfiguration, creationContext);

                ImplementRequestParametersInterface(typeBuilder, methodConfiguration, creationContext);

                var returnType = typeBuilder.CreateTypeInfo().AsType();

                creationContext.InitActions.ForEach(act => act(returnType));

                return returnType;
            }
        }

        protected virtual void GenerateConstructorWithDefaults(TypeBuilder typeBuilder, EndPointMethodConfiguration methodConfiguration, TypeCreationContext creationContext)
        {
            var constructorBuilder = typeBuilder.DefineConstructor(MethodAttributes.Public,
                CallingConventions.Standard, new Type[0]);

            var ilGenerator = constructorBuilder.GetILGenerator();

            foreach (var rpcParameterInfo in methodConfiguration.Parameters)
            {
                if (rpcParameterInfo.DefaultValue != null)
                {
                    var backingField = creationContext.ParameterInfos.First(p => p.Parameter == rpcParameterInfo).BackingField;

                    if (rpcParameterInfo.ParamType == typeof(int) || rpcParameterInfo.ParamType == typeof(int?))
                    {
                        GenerateIntDefault(typeBuilder, methodConfiguration, creationContext, ilGenerator,
                            rpcParameterInfo, backingField);
                    }
                    else if (rpcParameterInfo.ParamType == typeof(double) || rpcParameterInfo.ParamType == typeof(double?))
                    {
                        GenerateDoubleDefault(typeBuilder, methodConfiguration, creationContext, ilGenerator,
                            rpcParameterInfo, backingField);
                    }
                    else if (rpcParameterInfo.ParamType == typeof(bool) || rpcParameterInfo.ParamType == typeof(bool?))
                    {
                        GenerateBooleanDefault(typeBuilder, methodConfiguration, creationContext, ilGenerator,
                            rpcParameterInfo, backingField);
                    }
                    else if(rpcParameterInfo.ParamType == typeof(string))
                    {
                        GenerateStringDefault(typeBuilder, methodConfiguration, creationContext, ilGenerator,
                            rpcParameterInfo, backingField);
                    }
                    else
                    {
                        GenerateComplexDefault(typeBuilder, methodConfiguration, creationContext, ilGenerator,
                            rpcParameterInfo, backingField);
                    }
                }
            }

            ilGenerator.Emit(OpCodes.Ret);
        }

        protected virtual void GenerateIntDefault(TypeBuilder typeBuilder,
            EndPointMethodConfiguration methodConfiguration, TypeCreationContext creationContext,
            ILGenerator ilGenerator, RpcParameterInfo rpcParameterInfo, FieldBuilder backingField)
        {
            ilGenerator.Emit(OpCodes.Ldarg_0);
            ilGenerator.EmitInt((int)rpcParameterInfo.DefaultValue);
            ilGenerator.Emit(OpCodes.Stfld, backingField);
        }

        protected virtual void GenerateStringDefault(TypeBuilder typeBuilder, EndPointMethodConfiguration methodConfiguration, TypeCreationContext creationContext, ILGenerator ilGenerator, RpcParameterInfo rpcParameterInfo, FieldBuilder backingField)
        {
            ilGenerator.Emit(OpCodes.Ldarg_0);
            ilGenerator.EmitInt((int)rpcParameterInfo.DefaultValue);
            ilGenerator.Emit(OpCodes.Stfld, backingField);
        }
        
        protected virtual void GenerateDoubleDefault(TypeBuilder typeBuilder, EndPointMethodConfiguration methodConfiguration, TypeCreationContext creationContext, ILGenerator ilGenerator, RpcParameterInfo rpcParameterInfo, FieldBuilder backingField)
        {
            ilGenerator.Emit(OpCodes.Ldarg_0);
            ilGenerator.Emit(OpCodes.Ldc_R8, (double)rpcParameterInfo.DefaultValue);
            ilGenerator.Emit(OpCodes.Stfld, backingField);
        }

        protected virtual void GenerateBooleanDefault(TypeBuilder typeBuilder, EndPointMethodConfiguration methodConfiguration, TypeCreationContext creationContext, ILGenerator ilGenerator, RpcParameterInfo rpcParameterInfo, FieldBuilder backingField)
        {
            ilGenerator.Emit(OpCodes.Ldarg_0);
            ilGenerator.EmitBoolean((bool)rpcParameterInfo.DefaultValue);
            ilGenerator.Emit(OpCodes.Stfld, backingField);
        }

        protected virtual void GenerateComplexDefault(TypeBuilder typeBuilder,
            EndPointMethodConfiguration methodConfiguration, 
            TypeCreationContext creationContext,
            ILGenerator ilGenerator, 
            RpcParameterInfo rpcParameterInfo, 
            FieldBuilder backingField)
        {
            var defaultFieldName = "_internal_default_" + rpcParameterInfo.Name;

            var staticField = typeBuilder.DefineField(defaultFieldName,
                rpcParameterInfo.ParamType, FieldAttributes.Private | FieldAttributes.Static);

            ilGenerator.Emit(OpCodes.Ldarg_0);
            ilGenerator.Emit(OpCodes.Ldsfld, staticField);
            ilGenerator.Emit(OpCodes.Stfld, backingField);

            creationContext.InitActions.Add(type =>
            {
                var internalField = type.GetTypeInfo().GetField(defaultFieldName, BindingFlags.Static | BindingFlags.NonPublic);

                internalField.SetValue(null, rpcParameterInfo.DefaultValue);
            });
        }

        #region Constructor and fields
        protected virtual TypeBuilder CreateTypeBuilder(EndPointMethodConfiguration methodConfiguration,
            TypeCreationContext creationContext)
        {
            return _moduleBuilder.DefineType("DeserializeType" + _proxyCount, TypeAttributes.Public);
        }

        protected virtual void CreateBackingFields(TypeBuilder typeBuilder,
            EndPointMethodConfiguration methodConfiguration, TypeCreationContext creationContext)
        {
            foreach (var parameter in methodConfiguration.Parameters)
            {
                var backingField =
                    typeBuilder.DefineField("_" + parameter.Name, parameter.ParamType, FieldAttributes.Private);

                creationContext.ParameterInfos.Add(new InternalParameterInfo { BackingField = backingField, Parameter = parameter });
            }

            creationContext.StaticParameterInfo = typeBuilder.DefineField("_internal_parameterInfo",
                typeof(IReadOnlyList<IRpcParameterInfo>), FieldAttributes.Private | FieldAttributes.Static);

            creationContext.InitActions.Add(type =>
            {
                var internalField = type.GetTypeInfo().GetField("_internal_parameterInfo", BindingFlags.Static | BindingFlags.NonPublic);

                internalField.SetValue(null, methodConfiguration.Parameters);
            });
        }
        #endregion

        #region interface generation

        protected virtual void AddInterfaceImplementations(TypeBuilder typeBuilder,
            EndPointMethodConfiguration methodConfiguration, TypeCreationContext creationContext)
        {
            var closedInterface = typeof(IRequestParametersImplementations<>).MakeGenericType(typeBuilder);

            typeBuilder.AddInterfaceImplementation(closedInterface);
        }

        protected virtual void ImplementRequestParametersInterface(TypeBuilder typeBuilder, EndPointMethodConfiguration methodConfiguration, TypeCreationContext creationContext)
        {
            GenerateTryGetParameter(typeBuilder, methodConfiguration, creationContext);

            GenerateTrySetParameter(typeBuilder, methodConfiguration, creationContext);

            GenerateParameterNames(typeBuilder, methodConfiguration, creationContext);

            GenerateArrayAccess(typeBuilder, methodConfiguration, creationContext);

            GenerateNumberOfParameters(typeBuilder, methodConfiguration, creationContext);
        }

        protected virtual void GenerateNumberOfParameters(TypeBuilder typeBuilder, EndPointMethodConfiguration methodConfiguration, TypeCreationContext creationContext)
        {
            var methodAttr = MethodAttributes.Private | MethodAttributes.HideBySig |
                             MethodAttributes.NewSlot | MethodAttributes.Virtual |
                             MethodAttributes.Final;

            MethodBuilder numberOfParameters = typeBuilder.DefineMethod("get_Internal_ParameterCount",
                    methodAttr,
                    typeof(int),
                    Type.EmptyTypes);

            ILGenerator methodIL = numberOfParameters.GetILGenerator();

            methodIL.EmitInt(creationContext.ParameterInfos.Count);
            methodIL.Emit(OpCodes.Ret);

            var parameterCount = typeof(IRequestParameters).GetProperty(nameof(IRequestParameters.ParameterCount)).GetGetMethod();

            typeBuilder.DefineMethodOverride(numberOfParameters, parameterCount);
        }

        protected virtual void GenerateArrayAccess(TypeBuilder typeBuilder, EndPointMethodConfiguration methodConfiguration, TypeCreationContext creationContext)
        {
            GenerateGetItemMethod(typeBuilder, methodConfiguration, creationContext);

            GenerateSetItemMethod(typeBuilder, methodConfiguration, creationContext);
        }

        private void GenerateSetItemMethod(TypeBuilder typeBuilder, EndPointMethodConfiguration methodConfiguration, TypeCreationContext creationContext)
        {
            var setMethod = typeBuilder.DefineMethod("set_Item", MethodAttributes.Virtual | MethodAttributes.Private,
                null, new[] { typeof(int), typeof(object) });

            var methodIL = setMethod.GetILGenerator();

            var returnLabel = methodIL.DefineLabel();
            var defaultLabel = methodIL.DefineLabel();
            var caseLabels = methodConfiguration.Parameters.Select(p => methodIL.DefineLabel()).ToArray();

            methodIL.Emit(OpCodes.Ldarg_1);
            methodIL.Emit(OpCodes.Switch, caseLabels);

            methodIL.Emit(OpCodes.Br_S, defaultLabel);


            for (var i = 0; i < methodConfiguration.Parameters.Count; i++)
            {
                var parameterInfo = methodConfiguration.Parameters[i];

                methodIL.MarkLabel(caseLabels[i]);

                methodIL.Emit(OpCodes.Ldarg_0);
                methodIL.Emit(OpCodes.Ldarg_2);

                if (parameterInfo.ParamType.IsValueType)
                {
                    methodIL.Emit(OpCodes.Unbox_Any, parameterInfo.ParamType);
                }

                methodIL.Emit(OpCodes.Stfld, creationContext.ParameterInfos[i].BackingField);
                methodIL.Emit(OpCodes.Br_S, returnLabel);
            }

            var constructor = typeof(IndexOutOfRangeException).GetConstructor(new Type[0]);

            methodIL.MarkLabel(defaultLabel);
            methodIL.Emit(OpCodes.Newobj, constructor);
            methodIL.Emit(OpCodes.Throw);

            methodIL.MarkLabel(returnLabel);
            methodIL.Emit(OpCodes.Ret);

            var baseMethod = typeof(IRequestParameters).GetProperty("Item", typeof(object), new[] { typeof(int) }).GetSetMethod();

            typeBuilder.DefineMethodOverride(setMethod, baseMethod);
        }

        protected virtual void GenerateGetItemMethod(TypeBuilder typeBuilder, EndPointMethodConfiguration methodConfiguration, TypeCreationContext creationContext)
        {
            var getMethod = typeBuilder.DefineMethod("get_Item", MethodAttributes.Virtual | MethodAttributes.Private,
                typeof(object), new[] { typeof(int) });

            var methodIL = getMethod.GetILGenerator();

            var returnLabel = methodIL.DefineLabel();
            var defaultLabel = methodIL.DefineLabel();
            var caseLabels = methodConfiguration.Parameters.Select(p => methodIL.DefineLabel()).ToArray();

            methodIL.Emit(OpCodes.Ldarg_1);
            methodIL.Emit(OpCodes.Switch, caseLabels);

            methodIL.Emit(OpCodes.Br_S, defaultLabel);

            for (var i = 0; i < methodConfiguration.Parameters.Count; i++)
            {
                var parameterInfo = methodConfiguration.Parameters[i];

                methodIL.MarkLabel(caseLabels[i]);
                methodIL.Emit(OpCodes.Ldarg_0);
                methodIL.Emit(OpCodes.Ldfld, creationContext.ParameterInfos[i].BackingField);

                if (parameterInfo.ParamType.IsValueType)
                {
                    methodIL.Emit(OpCodes.Box, parameterInfo.ParamType);
                }
                methodIL.Emit(OpCodes.Br_S, returnLabel);
            }

            var constructor = typeof(IndexOutOfRangeException).GetConstructor(new Type[0]);

            methodIL.MarkLabel(defaultLabel);
            methodIL.Emit(OpCodes.Newobj, constructor);
            methodIL.Emit(OpCodes.Throw);

            methodIL.MarkLabel(returnLabel);
            methodIL.Emit(OpCodes.Ret);

            var baseMethod = typeof(IRequestParameters).GetProperty("Item", typeof(object), new[] { typeof(int) }).GetGetMethod();

            typeBuilder.DefineMethodOverride(getMethod, baseMethod);
        }

        protected virtual void GenerateParameterNames(TypeBuilder typeBuilder, EndPointMethodConfiguration methodConfiguration, TypeCreationContext creationContext)
        {
            var methodAttr = MethodAttributes.Private | MethodAttributes.HideBySig |
                             MethodAttributes.NewSlot | MethodAttributes.Virtual |
                             MethodAttributes.Final;

            MethodBuilder getParameterNames = typeBuilder.DefineMethod("get_Internal_ParameterInfos",
                methodAttr,
                typeof(IReadOnlyList<IRpcParameterInfo>),
                Type.EmptyTypes);

            ILGenerator methodIL = getParameterNames.GetILGenerator();

            methodIL.Emit(OpCodes.Ldsfld, creationContext.StaticParameterInfo);

            methodIL.Emit(OpCodes.Ret);

            var getMethod = typeof(IRequestParameters).GetProperty(nameof(IRequestParameters.ParameterInfos)).GetGetMethod();

            typeBuilder.DefineMethodOverride(getParameterNames, getMethod);
        }
        
        protected virtual void GenerateTrySetParameter(TypeBuilder typeBuilder, EndPointMethodConfiguration methodConfiguration, TypeCreationContext creationContext)
        {
            var methodAttr = MethodAttributes.Private | MethodAttributes.HideBySig |
                             MethodAttributes.NewSlot | MethodAttributes.Virtual |
                             MethodAttributes.Final;

            MethodBuilder trySetParameter = typeBuilder.DefineMethod("IRequestParameters.TrySetParameter",
                methodAttr,
                typeof(bool),
                new[] { typeof(string), typeof(object) });


            GenerateTrySetParameterIL(trySetParameter, methodConfiguration, creationContext);

            typeBuilder.DefineMethodOverride(trySetParameter, typeof(IRequestParameters).GetMethod(nameof(IRequestParameters.TrySetParameter)));
        }

        protected virtual void GenerateTrySetParameterIL(MethodBuilder tryGetParameter, EndPointMethodConfiguration methodConfiguration, TypeCreationContext creationContext)
        {
            ILGenerator methodIL = tryGetParameter.GetILGenerator();

            var localParamName = methodIL.DeclareLocal(typeof(string));
            var returnValue = methodIL.DeclareLocal(typeof(bool));

            var defaultCase = methodIL.DefineLabel();
            var returnLabel = methodIL.DefineLabel();

            var caseLabels = methodConfiguration.Parameters.Select(p => methodIL.DefineLabel()).ToArray();

            methodIL.Emit(OpCodes.Ldarg_1);
            methodIL.Emit(OpCodes.Stloc_0);

            // load local parameter
            methodIL.Emit(OpCodes.Ldloc_0);
            methodIL.Emit(OpCodes.Brfalse, defaultCase);

            for (int i = 0; i < methodConfiguration.Parameters.Count; i++)
            {
                methodIL.Emit(OpCodes.Ldloc_0);
                methodIL.Emit(OpCodes.Ldstr, methodConfiguration.Parameters[i].Name);
                methodIL.EmitMethodCall(_stringEqual);
                methodIL.Emit(OpCodes.Brtrue_S, caseLabels[i]);
            }

            methodIL.Emit(OpCodes.Br_S, defaultCase);

            for (int i = 0; i < methodConfiguration.Parameters.Count; i++)
            {
                var paramInfo = creationContext.ParameterInfos[i];

                methodIL.MarkLabel(caseLabels[i]);

                methodIL.Emit(OpCodes.Ldarg_0);
                methodIL.Emit(OpCodes.Ldarg_2);

                if (paramInfo.Parameter.ParamType.IsValueType)
                {
                    methodIL.Emit(OpCodes.Unbox_Any, paramInfo.Parameter.ParamType);
                }

                methodIL.Emit(OpCodes.Stfld, creationContext.ParameterInfos[i].BackingField);

                methodIL.EmitInt(1);
                methodIL.Emit(OpCodes.Stloc_1);
                methodIL.Emit(OpCodes.Br_S, returnLabel);
            }

            methodIL.MarkLabel(defaultCase);

            methodIL.EmitInt(0);
            methodIL.Emit(OpCodes.Stloc_1);
            methodIL.Emit(OpCodes.Br_S, returnLabel);

            methodIL.MarkLabel(returnLabel);
            methodIL.Emit(OpCodes.Ldloc_1);
            methodIL.Emit(OpCodes.Ret);
        }

        protected virtual void GenerateTryGetParameter(TypeBuilder typeBuilder, EndPointMethodConfiguration methodConfiguration, TypeCreationContext creationContext)
        {
            var methodAttr = MethodAttributes.Private | MethodAttributes.HideBySig |
                             MethodAttributes.NewSlot | MethodAttributes.Virtual |
                             MethodAttributes.Final;

            MethodBuilder tryGetParameter = typeBuilder.DefineMethod("IRequestParameters.TryGetParameter",
                methodAttr,
                typeof(bool),
                new[] { typeof(string), typeof(object).MakeByRefType() });

            tryGetParameter.DefineParameter(2, ParameterAttributes.Out, "parameterValue");

            GenerateTryGetParameterIL(tryGetParameter, methodConfiguration, creationContext);

            typeBuilder.DefineMethodOverride(tryGetParameter, typeof(IRequestParameters).GetMethod(nameof(IRequestParameters.TryGetParameter)));
        }

        protected virtual void GenerateTryGetParameterIL(MethodBuilder tryGetParameter,
            EndPointMethodConfiguration methodConfiguration, TypeCreationContext creationContext)
        {
            ILGenerator methodIL = tryGetParameter.GetILGenerator();

            var localParamName = methodIL.DeclareLocal(typeof(string));
            var returnValue = methodIL.DeclareLocal(typeof(bool));

            var defaultCase = methodIL.DefineLabel();
            var returnLabel = methodIL.DefineLabel();

            var caseLabels = methodConfiguration.Parameters.Select(p => methodIL.DefineLabel()).ToArray();

            methodIL.Emit(OpCodes.Ldarg_1);
            methodIL.Emit(OpCodes.Stloc_0);

            // load local parameter
            methodIL.Emit(OpCodes.Ldloc_0);
            methodIL.Emit(OpCodes.Brfalse, defaultCase);

            for (int i = 0; i < methodConfiguration.Parameters.Count; i++)
            {
                methodIL.Emit(OpCodes.Ldloc_0);
                methodIL.Emit(OpCodes.Ldstr, methodConfiguration.Parameters[i].Name);
                methodIL.EmitMethodCall(_stringEqual);
                methodIL.Emit(OpCodes.Brtrue_S, caseLabels[i]);
            }

            methodIL.Emit(OpCodes.Br_S, defaultCase);

            for (int i = 0; i < methodConfiguration.Parameters.Count; i++)
            {
                var paramInfo = creationContext.ParameterInfos[i];

                methodIL.MarkLabel(caseLabels[i]);
                methodIL.Emit(OpCodes.Ldarg_2);
                methodIL.Emit(OpCodes.Ldarg_0);
                methodIL.Emit(OpCodes.Ldfld, paramInfo.BackingField);

                if (paramInfo.Parameter.ParamType.IsValueType)
                {
                    methodIL.Emit(OpCodes.Box, paramInfo.Parameter.ParamType);
                }

                methodIL.Emit(OpCodes.Stind_Ref);

                methodIL.EmitInt(1);
                methodIL.Emit(OpCodes.Stloc_1);
                methodIL.Emit(OpCodes.Br_S, returnLabel);
            }

            methodIL.MarkLabel(defaultCase);
            methodIL.Emit(OpCodes.Ldarg_2);
            methodIL.Emit(OpCodes.Ldnull);
            methodIL.Emit(OpCodes.Stind_Ref);

            methodIL.EmitInt(0);
            methodIL.Emit(OpCodes.Stloc_1);
            methodIL.Emit(OpCodes.Br_S, returnLabel);

            methodIL.MarkLabel(returnLabel);
            methodIL.Emit(OpCodes.Ldloc_1);
            methodIL.Emit(OpCodes.Ret);
        }

        #endregion

        #region Property implementation

        protected virtual void ImplementProperties(TypeBuilder typeBuilder, EndPointMethodConfiguration methodConfiguration, TypeCreationContext creationContext)
        {
            foreach (var parameterInfo in creationContext.ParameterInfos)
            {
                PropertyBuilder propertyBuilder = typeBuilder.DefineProperty(parameterInfo.Parameter.Name,
                    PropertyAttributes.HasDefault,
                    parameterInfo.Parameter.ParamType,
                    null);

                GeneratePropertyGet(typeBuilder, propertyBuilder, parameterInfo, methodConfiguration, creationContext);

                GeneratePropertySet(typeBuilder, propertyBuilder, parameterInfo, methodConfiguration, creationContext);
            }
        }

        protected virtual void GeneratePropertySet(TypeBuilder typeBuilder, PropertyBuilder propertyBuilder, InternalParameterInfo parameterInfo, EndPointMethodConfiguration methodConfiguration, TypeCreationContext creationContext)
        {
            // The property set and property get methods require a special
            // set of attributes.
            MethodAttributes setAttr =
                MethodAttributes.Public | MethodAttributes.SpecialName |
                MethodAttributes.HideBySig;

            // Define the "get" accessor method for CustomerName.
            MethodBuilder setMethodBuilder =
                typeBuilder.DefineMethod("set_" + parameterInfo.Parameter.Name,
                    setAttr,
                    null,
                    new Type[] { parameterInfo.Parameter.ParamType });

            GeneratePropertySetIL(typeBuilder, setMethodBuilder, parameterInfo, methodConfiguration, creationContext);

            propertyBuilder.SetSetMethod(setMethodBuilder);
        }

        protected virtual void GeneratePropertySetIL(TypeBuilder typeBuilder, MethodBuilder setMethodBuilder, InternalParameterInfo parameterInfo, EndPointMethodConfiguration methodConfiguration, TypeCreationContext creationContext)
        {
            ILGenerator setIL = setMethodBuilder.GetILGenerator();

            setIL.Emit(OpCodes.Ldarg_0);
            setIL.Emit(OpCodes.Ldarg_1);
            setIL.Emit(OpCodes.Stfld, parameterInfo.BackingField);
            setIL.Emit(OpCodes.Ret);
        }

        protected virtual void GeneratePropertyGet(TypeBuilder typeBuilder, PropertyBuilder propertyBuilder, InternalParameterInfo parameterInfo, EndPointMethodConfiguration methodConfiguration, TypeCreationContext creationContext)
        {
            // The property set and property get methods require a special
            // set of attributes.
            MethodAttributes getAttr =
                MethodAttributes.Public | MethodAttributes.SpecialName |
                MethodAttributes.HideBySig;

            // Define the "get" accessor method for CustomerName.
            MethodBuilder getMethodBuilder =
                typeBuilder.DefineMethod("get_" + parameterInfo.Parameter.Name,
                    getAttr,
                    parameterInfo.Parameter.ParamType,
                    Type.EmptyTypes);

            GeneratePropertyGetIL(typeBuilder, getMethodBuilder, parameterInfo, methodConfiguration, creationContext);

            propertyBuilder.SetGetMethod(getMethodBuilder);
        }

        protected virtual void GeneratePropertyGetIL(TypeBuilder typeBuilder,
            MethodBuilder getMethodBuilder,
            InternalParameterInfo parameterInfo,
            EndPointMethodConfiguration methodConfiguration,
            TypeCreationContext creationContext)
        {
            ILGenerator getIL = getMethodBuilder.GetILGenerator();

            getIL.Emit(OpCodes.Ldarg_0);
            getIL.Emit(OpCodes.Ldfld, parameterInfo.BackingField);
            getIL.Emit(OpCodes.Ret);
        }

        #endregion

        #region context classes
        public class TypeCreationContext
        {
            public FieldBuilder StaticParameterInfo { get; set; }

            public List<InternalParameterInfo> ParameterInfos { get; } = new List<InternalParameterInfo>();

            public List<Action<Type>> InitActions { get; } = new List<Action<Type>>();
        }

        public class InternalParameterInfo
        {
            public RpcParameterInfo Parameter { get; set; }

            public FieldBuilder BackingField { get; set; }
        }

        #endregion
    }
}
