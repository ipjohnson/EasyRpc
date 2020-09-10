using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using EasyRpc.AspNetCore.EndPoints;
using EasyRpc.AspNetCore.Errors;

namespace EasyRpc.AspNetCore.CodeGeneration
{
    /// <summary>
    /// Creates Error result types that are serialized to client
    /// </summary>
    public interface IErrorResultTypeCreator
    {
        /// <summary>
        /// Generate a new error type
        /// </summary>
        /// <returns></returns>
        Type GenerateErrorType();
    }

    /// <inheritdoc />
    public class ErrorResultTypeCreator : IErrorResultTypeCreator
    {
        private ModuleBuilder _moduleBuilder;
        private Type _errorType;
        private object _lock = new object();
        private IEnumerable<ISerializationTypeAttributor> _serializationTypeAttributors;

        /// <summary>
        /// Default constructor
        /// </summary>
        public ErrorResultTypeCreator(IEnumerable<ISerializationTypeAttributor> serializationTypeAttributors)
        {
            _serializationTypeAttributors = serializationTypeAttributors;
            var dynamicAssembly = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName(Guid.NewGuid().ToString()), AssemblyBuilderAccess.Run);

            _moduleBuilder = dynamicAssembly.DefineDynamicModule("ErrorType");
        }

        /// <inheritdoc />
        public Type GenerateErrorType() => _errorType ??= InternalGenerateErrorType();

        /// <summary>
        /// Creates type used to serialize error
        /// </summary>
        /// <returns></returns>
        protected virtual Type InternalGenerateErrorType()
        {
            lock (_lock)
            {
                if (_errorType == null)
                {
                    var typeBuilder = CreateTypeBuilder();

                    AddAttributes(typeBuilder);

                    AddInterfaces(typeBuilder);

                    AddProperties(typeBuilder);

                    _errorType = typeBuilder.CreateTypeInfo().AsType();
                }
            }

            return _errorType;
        }

        private void AddAttributes(TypeBuilder typeBuilder)
        {
            foreach (var attributor in _serializationTypeAttributors)
            {
                attributor.AttributeType(typeBuilder, "error");
            }
        }

        protected virtual void AddProperties(TypeBuilder typeBuilder)
        {
            var backingField =
                typeBuilder.DefineField("_errorMessage", typeof(string), FieldAttributes.Private);

            PropertyBuilder propertyBuilder = typeBuilder.DefineProperty("Message",
                PropertyAttributes.HasDefault,
                typeof(string),
                null);

            GenerateMessagePropertyGet(typeBuilder, propertyBuilder, backingField);

            GenerateMessagePropertySet(typeBuilder, propertyBuilder, backingField);

            foreach (var attributor in _serializationTypeAttributors)
            {
                attributor.AttributeProperty(propertyBuilder,0);
            }
        }

        private void GenerateMessagePropertySet(TypeBuilder typeBuilder, PropertyBuilder propertyBuilder,
            FieldBuilder backingField)
        {
            var methodAttr = MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.Virtual |
                             MethodAttributes.HideBySig;

            MethodBuilder setMethod = typeBuilder.DefineMethod("set_Message",
                methodAttr,
                typeof(void),
                new [] {typeof(string)});

            var ilGenerator = setMethod.GetILGenerator();

            ilGenerator.Emit(OpCodes.Ldarg_0);
            ilGenerator.Emit(OpCodes.Ldarg_1);
            ilGenerator.Emit(OpCodes.Stfld, backingField);
            ilGenerator.Emit(OpCodes.Ret);

            propertyBuilder.SetSetMethod(setMethod);
        }

        private void GenerateMessagePropertyGet(TypeBuilder typeBuilder, PropertyBuilder propertyBuilder,
            FieldBuilder backingField)
        {
            var methodAttr = MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.Virtual |
                             MethodAttributes.HideBySig;

            MethodBuilder getMethod = typeBuilder.DefineMethod("get_Message",
                methodAttr,
                typeof(string),
                new Type[0]);
            
            var ilGenerator = getMethod.GetILGenerator();

            ilGenerator.Emit(OpCodes.Ldarg_0);
            ilGenerator.Emit(OpCodes.Ldfld, backingField);
            ilGenerator.Emit(OpCodes.Ret);
            
            propertyBuilder.SetGetMethod(getMethod);
        }

        protected virtual void AddInterfaces(TypeBuilder typeBuilder)
        {
            typeBuilder.AddInterfaceImplementation(typeof(IErrorWrapper));
        }

        protected virtual TypeBuilder CreateTypeBuilder()
        {
            return _moduleBuilder.DefineType("ErrorResponse", TypeAttributes.Public);
        }

    }
}
