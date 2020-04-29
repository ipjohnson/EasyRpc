using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using NullReferenceException = System.NullReferenceException;

namespace EasyRpc.DynamicClient.CodeGeneration
{
    public interface ISerializationTypeCreator
    {
        Type CreateSerializationTypeForMethod(List<ParameterInfo> parameters);
    }


    public class SerializationTypeCreator : ISerializationTypeCreator
    {
        private readonly object _lock = new object();
        private readonly ModuleBuilder _moduleBuilder;
        private int _proxyCount = 0;

        public SerializationTypeCreator()
        {
            var dynamicAssembly = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName(Guid.NewGuid().ToString()), AssemblyBuilderAccess.Run);

            _moduleBuilder = dynamicAssembly.DefineDynamicModule("SerializeTypes");
        }


        public Type CreateSerializationTypeForMethod(List<ParameterInfo> parameters)
        {
            lock (_lock)
            {
                var typeBuilder = CreateTypeBuilder();

                GenerateParameters(typeBuilder, parameters);

                return typeBuilder.CreateTypeInfo().AsType();
            }
        }

        private void GenerateParameters(TypeBuilder typeBuilder, List<ParameterInfo> parameters)
        {
            foreach (var parameterInfo in parameters)
            {
                GenerateProperty(typeBuilder, parameterInfo);
            }
        }

        private void GenerateProperty(TypeBuilder typeBuilder, ParameterInfo parameterInfo)
        {
            var backingField =
                typeBuilder.DefineField("_" + parameterInfo.Name, parameterInfo.ParameterType, FieldAttributes.Private);

            var propertyBuilder = typeBuilder.DefineProperty(parameterInfo.Name,
                PropertyAttributes.HasDefault,
                parameterInfo.ParameterType,
                null);

            GeneratePropertyGetMethod(typeBuilder, propertyBuilder, backingField, parameterInfo);

            GeneratePropertySetMethod(typeBuilder, propertyBuilder, backingField, parameterInfo);
        }

        private void GeneratePropertySetMethod(TypeBuilder typeBuilder, PropertyBuilder propertyBuilder,
            FieldBuilder backingField, ParameterInfo parameterInfo)
        {
            // The property set and property get methods require a special
            // set of attributes.
            MethodAttributes setAttr =
                MethodAttributes.Public | MethodAttributes.SpecialName |
                MethodAttributes.HideBySig;

            // Define the "get" accessor method for CustomerName.
            MethodBuilder setMethodBuilder =
                typeBuilder.DefineMethod("set_" + parameterInfo.Name,
                    setAttr,
                    null,
                    new [] { parameterInfo.ParameterType });

            GeneratePropertySetIL(typeBuilder, setMethodBuilder, parameterInfo,backingField);

            propertyBuilder.SetSetMethod(setMethodBuilder);
        }

        private void GeneratePropertySetIL(TypeBuilder typeBuilder, MethodBuilder setMethodBuilder, ParameterInfo parameterInfo, FieldBuilder backingField)
        {
            ILGenerator setIL = setMethodBuilder.GetILGenerator();

            setIL.Emit(OpCodes.Ldarg_0);
            setIL.Emit(OpCodes.Ldarg_1);
            setIL.Emit(OpCodes.Stfld, backingField);
            setIL.Emit(OpCodes.Ret);
        }

        private void GeneratePropertyGetMethod(TypeBuilder typeBuilder, PropertyBuilder propertyBuilder,
            FieldBuilder backingField, ParameterInfo parameterInfo)
        {
            // The property set and property get methods require a special
            // set of attributes.
            MethodAttributes getAttr =
                MethodAttributes.Public | MethodAttributes.SpecialName |
                MethodAttributes.HideBySig;

            // Define the "get" accessor method for CustomerName.
            MethodBuilder getMethodBuilder =
                typeBuilder.DefineMethod("get_" + parameterInfo.Name,
                    getAttr,
                    parameterInfo.ParameterType,
                    Type.EmptyTypes);

            GeneratePropertyGetIL(typeBuilder, getMethodBuilder, parameterInfo, backingField);

            propertyBuilder.SetGetMethod(getMethodBuilder);
        }

        private void GeneratePropertyGetIL(TypeBuilder typeBuilder, MethodBuilder getMethodBuilder,
            ParameterInfo parameterInfo, FieldBuilder backingField)
        {
            ILGenerator getIL = getMethodBuilder.GetILGenerator();

            getIL.Emit(OpCodes.Ldarg_0);
            getIL.Emit(OpCodes.Ldfld, backingField);
            getIL.Emit(OpCodes.Ret);
        }

        private TypeBuilder CreateTypeBuilder()
        {
            return _moduleBuilder.DefineType("SerializeType" + _proxyCount, TypeAttributes.Public);
        }
    }
}
