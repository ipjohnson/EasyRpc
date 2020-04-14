using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using EasyRpc.AspNetCore.EndPoints;

namespace EasyRpc.AspNetCore.CodeGeneration
{
    public interface IWrappedResultTypeCreator
    {
        Type GetTypeWrapper(Type typeToWrap);
    }

    public class WrappedResultTypeCreator : IWrappedResultTypeCreator
    {
        private readonly object _lock = new object();
        private readonly ModuleBuilder _moduleBuilder;
        private int _proxyCount = 0;
        private readonly ConcurrentDictionary<Type,Type> _wrappers = new ConcurrentDictionary<Type, Type>();

        public WrappedResultTypeCreator()
        {

            var dynamicAssembly = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName(Guid.NewGuid().ToString()), AssemblyBuilderAccess.Run);

            _moduleBuilder = dynamicAssembly.DefineDynamicModule("TypeWrapper");
        }

        public Type GetTypeWrapper(Type typeToWrap)
        {
            if (_wrappers.TryGetValue(typeToWrap, out var wrappedType))
            {
                return wrappedType;
            }
            
            return CreateWrapper(typeToWrap);
        }

        protected virtual Type CreateWrapper(Type typeToWrap)
        {
            Type wrappedType = null;

            lock (_lock)
            {
                _proxyCount++;

                var wrapperTypeBuilder = CreateTypeBuilder(typeToWrap);

                AddInterfaceImplementations(wrapperTypeBuilder, typeToWrap);

                AddValueProperty(wrapperTypeBuilder, typeToWrap);

                wrappedType = wrapperTypeBuilder.CreateTypeInfo().AsType();
            }

            _wrappers.TryAdd(typeToWrap, wrappedType);

            return wrappedType;
        }
        
        protected virtual TypeBuilder CreateTypeBuilder(Type typeToWrap)
        {
            return _moduleBuilder.DefineType($"{typeToWrap.Name}Wrapper" + _proxyCount, TypeAttributes.Public);
        }

        protected virtual void AddInterfaceImplementations(TypeBuilder typeBuilder, Type typeToWrap)
        {
            var closedInterface = typeof(IResultWrapper<>).MakeGenericType(typeToWrap);

            typeBuilder.AddInterfaceImplementation(closedInterface);
        }
        
        protected virtual void AddValueProperty(TypeBuilder typeBuilder, Type typeToWrap)
        {
            var backingField =
                typeBuilder.DefineField("_result", typeToWrap, FieldAttributes.Private);

            PropertyBuilder propertyBuilder = typeBuilder.DefineProperty("Result",
                PropertyAttributes.HasDefault,
                typeToWrap,
                null);

            GeneratePropertyGet(typeBuilder, propertyBuilder, typeToWrap, backingField);

            GeneratePropertySet(typeBuilder, propertyBuilder, typeToWrap, backingField);

            var closedInterface = typeof(IResultWrapper<>).MakeGenericType(typeToWrap);

            var interfaceProperty = closedInterface.GetProperty(nameof(IResultWrapper<int>.Result));

            typeBuilder.DefineMethodOverride(propertyBuilder.GetMethod, interfaceProperty.GetMethod);
            typeBuilder.DefineMethodOverride(propertyBuilder.SetMethod, interfaceProperty.SetMethod);
        }

        protected virtual void GeneratePropertySet(TypeBuilder typeBuilder, PropertyBuilder propertyBuilder,
            Type typeToWrap, FieldBuilder backingField)
        {
            // The property set and property get methods require a special
            // set of attributes.
            MethodAttributes setAttr = MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig | MethodAttributes.Virtual;

            // Define the "get" accessor method for CustomerName.
            MethodBuilder setMethodBuilder =
                typeBuilder.DefineMethod("set_Result",
                    setAttr,
                    null,
                    new Type[] { typeToWrap });

            GeneratePropertySetIL(typeBuilder, setMethodBuilder, backingField);

            propertyBuilder.SetSetMethod(setMethodBuilder);
        }

        protected virtual void GeneratePropertySetIL(TypeBuilder typeBuilder, MethodBuilder setMethodBuilder,
            FieldBuilder backingField)
        {
            ILGenerator setIL = setMethodBuilder.GetILGenerator();

            setIL.Emit(OpCodes.Ldarg_0);
            setIL.Emit(OpCodes.Ldarg_1);
            setIL.Emit(OpCodes.Stfld, backingField);
            setIL.Emit(OpCodes.Ret);
        }

        protected virtual void GeneratePropertyGet(TypeBuilder typeBuilder, PropertyBuilder propertyBuilder,
            Type typeToWrap, FieldBuilder backingField)
        {
            // The property set and property get methods require a special
            // set of attributes.
            MethodAttributes getAttr = MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig | MethodAttributes.Virtual;

            // Define the "get" accessor method for CustomerName.
            MethodBuilder getMethodBuilder =
                typeBuilder.DefineMethod("get_Result",
                    getAttr,
                    typeToWrap,
                    Type.EmptyTypes);

            GeneratePropertyGetIL(typeBuilder, getMethodBuilder, backingField);

            propertyBuilder.SetGetMethod(getMethodBuilder);
        }

        protected virtual void GeneratePropertyGetIL(TypeBuilder typeBuilder,
            MethodBuilder getMethodBuilder, FieldBuilder backingField)
        {
            ILGenerator getIL = getMethodBuilder.GetILGenerator();

            getIL.Emit(OpCodes.Ldarg_0);
            getIL.Emit(OpCodes.Ldfld, backingField);
            getIL.Emit(OpCodes.Ret);
        }
    }
}
