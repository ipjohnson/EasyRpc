using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace EasyRpc.DynamicClient.ProxyGenerator
{
    public interface IProxyGenerator
    {
        Type GenerateProxyType(Type proxyType, bool callByName);
    }

    public class ProxyGenerator : IProxyGenerator
    {
        private object _typeCreateLock = new object();
        private int _proxyCount = 0;
        private ModuleBuilder _moduleBuilder;
        private INamingConventionService _namingConventionService;

        public ProxyGenerator(INamingConventionService namingConventionService)
        {
            _namingConventionService = namingConventionService;

            SetupDyanmicAssembly();
        }

        private void SetupDyanmicAssembly()
        {
            var dynamicAssembly = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName(Guid.NewGuid().ToString()), AssemblyBuilderAccess.Run);

            _moduleBuilder = dynamicAssembly.DefineDynamicModule("DynamicProxyTypes");
        }

        public Type GenerateProxyType(Type proxyType, bool callByName)
        {
            lock (_typeCreateLock)
            {
                _proxyCount++;

                var proxyBuilder = _moduleBuilder.DefineType(proxyType.Name + "Proxy" + _proxyCount, 
                    TypeAttributes.Class | TypeAttributes.Public);

                proxyBuilder.AddInterfaceImplementation(proxyType);

                var callServiceField = proxyBuilder.DefineField("_callService", typeof(IRpcProxyService),
                    FieldAttributes.Private);
                var jsonMethodWriterField = proxyBuilder.DefineField("_jsonMethodWriter",
                    typeof(IJsonMethodObjectWriter), FieldAttributes.Private);
                var serializerField = proxyBuilder.DefineField("_serializer", typeof(JsonSerializer),
                    FieldAttributes.Private);

                var proxyConstructor = proxyBuilder.DefineConstructor(MethodAttributes.Public,
                    CallingConventions.Standard,
                    new[] { typeof(IRpcProxyService), typeof(IJsonMethodObjectWriter), typeof(JsonSerializer) });
                var proxyConstructorILGenerator = proxyConstructor.GetILGenerator();

                proxyConstructorILGenerator.Emit(OpCodes.Ldarg_0);
                proxyConstructorILGenerator.Emit(OpCodes.Ldarg_1);
                proxyConstructorILGenerator.Emit(OpCodes.Stfld, callServiceField);

                proxyConstructorILGenerator.Emit(OpCodes.Ldarg_0);
                proxyConstructorILGenerator.Emit(OpCodes.Ldarg_2);
                proxyConstructorILGenerator.Emit(OpCodes.Stfld, jsonMethodWriterField);

                proxyConstructorILGenerator.Emit(OpCodes.Ldarg_0);
                proxyConstructorILGenerator.Emit(OpCodes.Ldarg_3);
                proxyConstructorILGenerator.Emit(OpCodes.Stfld, serializerField);
                proxyConstructorILGenerator.Emit(OpCodes.Ret);

                foreach (var methodInfo in proxyType.GetRuntimeMethods())
                {
                    if (methodInfo.IsVirtual)
                    {
                        GenerateMethodProxy(proxyBuilder, proxyType, methodInfo, callByName, callServiceField,
                            serializerField, jsonMethodWriterField);
                    }
                }

                return proxyBuilder.CreateTypeInfo().AsType();
            }
        }

        private void GenerateMethodProxy(TypeBuilder proxyBuilder, Type type, MethodInfo method, bool callByName, FieldBuilder callService, FieldBuilder serializer, FieldBuilder jsonMethodWriterField)
        {
            var methodAttributes = MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.Virtual | MethodAttributes.NewSlot;

            var methodBuilder = proxyBuilder.DefineMethod(method.Name, methodAttributes, method.ReturnType, method.GetParameters().Select(x => x.ParameterType).ToArray());

            var ilGenerator = methodBuilder.GetILGenerator();

            ilGenerator.DeclareLocal(typeof(byte[])); // LdLoc_0
            ilGenerator.DeclareLocal(typeof(MemoryStream)); // LdLoc_1
            ilGenerator.DeclareLocal(typeof(StreamWriter)); // LdLoc_2
            ilGenerator.DeclareLocal(typeof(JsonTextWriter)); // LdLoc_3

            EmitMemoryStream(ilGenerator);

            EmitStreamWriter(ilGenerator);

            EmitJsonWriter(ilGenerator);

            EmitJsonOpenObject(ilGenerator, type, method, jsonMethodWriterField);

            if (callByName)
            {
                EmitParametersByName(ilGenerator, method, serializer);
            }
            else
            {
                EmitParametersByOrder(ilGenerator, method, serializer);
            }

            EmitIdAndJsonObjectClose(ilGenerator, jsonMethodWriterField);

            EmitJsonWriterFinallyDispose(ilGenerator);

            EmitStreamWriterDispose(ilGenerator);

            EmitCreateBytes(ilGenerator);

            EmitMemoryStreamFinallyDispose(ilGenerator);

            EmitMakeCall(ilGenerator, method, callService);

            proxyBuilder.DefineMethodOverride(methodBuilder, method);
        }

        private void EmitParametersByOrder(ILGenerator ilGenerator, MethodInfo method, FieldBuilder serializer)
        {
            ilGenerator.Emit(OpCodes.Ldloc_3);
            ilGenerator.Emit(OpCodes.Ldstr, "params");
            ilGenerator.Emit(OpCodes.Ldc_I4_0);
            ilGenerator.Emit(OpCodes.Callvirt, _writeParam);

            ilGenerator.Emit(OpCodes.Ldloc_3);
            ilGenerator.Emit(OpCodes.Callvirt, typeof(JsonTextWriter).GetMethod("WriteStartArray"));

            int parameterIndex = 0;
            foreach (var parameter in method.GetParameters())
            {
                EmitParameterWrite(ilGenerator, parameter, parameterIndex, serializer);
                parameterIndex++;
            }

            ilGenerator.Emit(OpCodes.Ldloc_3);
            ilGenerator.Emit(OpCodes.Callvirt, typeof(JsonTextWriter).GetMethod("WriteEndArray"));
        }

        private void EmitParametersByName(ILGenerator ilGenerator, MethodInfo method, FieldBuilder serializer)
        {
            ilGenerator.Emit(OpCodes.Ldloc_3);
            ilGenerator.Emit(OpCodes.Ldstr, "params");
            ilGenerator.Emit(OpCodes.Ldc_I4_0);
            ilGenerator.Emit(OpCodes.Callvirt, _writeParam);

            ilGenerator.Emit(OpCodes.Ldloc_3);
            ilGenerator.Emit(OpCodes.Callvirt, typeof(JsonTextWriter).GetMethod("WriteStartObject"));

            int parameterIndex = 0;
            foreach (var parameter in method.GetParameters())
            {
                ilGenerator.Emit(OpCodes.Ldloc_3);
                ilGenerator.Emit(OpCodes.Ldstr, parameter.Name);
                ilGenerator.Emit(OpCodes.Ldc_I4_0);
                ilGenerator.Emit(OpCodes.Callvirt, _writeParam);

                EmitParameterWrite(ilGenerator, parameter, parameterIndex, serializer);
                parameterIndex++;
            }

            ilGenerator.Emit(OpCodes.Ldloc_3);
            ilGenerator.Emit(OpCodes.Callvirt, typeof(JsonTextWriter).GetMethod("WriteEndObject"));
        }

        private void EmitJsonOpenObject(ILGenerator ilGenerator, Type type, MethodInfo method, FieldBuilder jsonMethodWriter)
        {
            ilGenerator.EmitLoadArg(0);
            ilGenerator.Emit(OpCodes.Ldfld, jsonMethodWriter);
            ilGenerator.Emit(OpCodes.Ldloc_3);
            ilGenerator.Emit(OpCodes.Ldstr, _namingConventionService.GetMethodName(method));
            ilGenerator.Emit(OpCodes.Callvirt, typeof(IJsonMethodObjectWriter).GetMethod("OpenMethodObject"));
        }

        private void EmitIdAndJsonObjectClose(ILGenerator ilGenerator, FieldBuilder jsonMethodWriter)
        {
            ilGenerator.EmitLoadArg(0);
            ilGenerator.Emit(OpCodes.Ldfld, jsonMethodWriter);
            ilGenerator.Emit(OpCodes.Ldloc_3);
            ilGenerator.Emit(OpCodes.Callvirt, typeof(IJsonMethodObjectWriter).GetMethod("CloseMethodObject"));
        }

        private void EmitMakeCall(ILGenerator ilGenerator, MethodInfo method, FieldBuilder callService)
        {
            MethodInfo methodToCall;

            if (method.ReturnType == typeof(void))
            {
                methodToCall = typeof(IRpcProxyService).GetMethod("MakeCallNoReturn");
            }
            else if (method.ReturnType == typeof(Task))
            {
                methodToCall = typeof(IRpcProxyService).GetMethod("MaskAsyncCallNoReturn");
            }
            else if (method.ReturnType.IsConstructedGenericType &&
                     method.ReturnType.GetGenericTypeDefinition() == typeof(Task<>))
            {
                methodToCall =
                    typeof(IRpcProxyService).GetMethod("MakeAsyncCallWithReturn")
                        .MakeGenericMethod(method.ReturnType.GenericTypeArguments);
            }
            else
            {
                methodToCall = typeof(IRpcProxyService).GetMethod("MakeCallWithReturn").MakeGenericMethod(method.ReturnType);
            }

            ilGenerator.Emit(OpCodes.Ldarg_0);
            ilGenerator.Emit(OpCodes.Ldfld, callService);
            ilGenerator.Emit(OpCodes.Ldstr, _namingConventionService.GetNameForType(method.DeclaringType));
            ilGenerator.Emit(OpCodes.Ldstr, _namingConventionService.GetMethodName(method));
            ilGenerator.Emit(OpCodes.Ldloc_0);
            ilGenerator.Emit(OpCodes.Callvirt, methodToCall);

            if (methodToCall.ReturnType == typeof(void))
            {
                ilGenerator.Emit(OpCodes.Pop);
            }

            ilGenerator.Emit(OpCodes.Ret);
        }

        private void EmitMemoryStreamFinallyDispose(ILGenerator ilGenerator)
        {
            ilGenerator.BeginFinallyBlock();

            ilGenerator.Emit(OpCodes.Ldloc_1);
            ilGenerator.Emit(OpCodes.Castclass, typeof(IDisposable));
            var disposeMethod = typeof(IDisposable).GetRuntimeMethod("Dispose", new Type[0]);

            ilGenerator.Emit(OpCodes.Callvirt, disposeMethod);

            ilGenerator.EndExceptionBlock();
        }

        private void EmitCreateBytes(ILGenerator ilGenerator)
        {
            ilGenerator.Emit(OpCodes.Ldloc_1);

            var method = typeof(MemoryStream).GetRuntimeMethod("ToArray", new Type[0]);

            ilGenerator.Emit(OpCodes.Callvirt, method);
            ilGenerator.Emit(OpCodes.Stloc_0);
        }

        private void EmitStreamWriterDispose(ILGenerator ilGenerator)
        {
            ilGenerator.BeginFinallyBlock();

            ilGenerator.Emit(OpCodes.Ldloc_2);
            ilGenerator.Emit(OpCodes.Castclass, typeof(IDisposable));
            ilGenerator.Emit(OpCodes.Callvirt, typeof(IDisposable).GetRuntimeMethod("Dispose", new Type[0]));

            ilGenerator.EndExceptionBlock();
        }

        private void EmitJsonWriterFinallyDispose(ILGenerator ilGenerator)
        {
            ilGenerator.BeginFinallyBlock();

            ilGenerator.Emit(OpCodes.Ldloc_2);
            ilGenerator.Emit(OpCodes.Castclass, typeof(IDisposable));
            ilGenerator.Emit(OpCodes.Callvirt, typeof(IDisposable).GetRuntimeMethod("Dispose", new Type[0]));

            ilGenerator.EndExceptionBlock();
        }

        private void EmitParameterWrite(ILGenerator ilGenerator, ParameterInfo parameter, int parameterIndex, FieldBuilder serializer)
        {
            var parameterTypeInfo = parameter.ParameterType.GetTypeInfo();

            var valueWritten = false;

            if (parameterTypeInfo.IsValueType ||
                parameter.ParameterType == typeof(string) ||
                (parameterTypeInfo.IsGenericType && parameterTypeInfo.GetGenericTypeDefinition() == typeof(Nullable<>)))
            {
                valueWritten = TryToWriteValue(ilGenerator, parameter, parameterIndex);
            }

            if (!valueWritten)
            {
                WriteValueWithSerializer(ilGenerator, parameter, parameterIndex, serializer);
            }
        }

        private void WriteValueWithSerializer(ILGenerator ilGenerator, ParameterInfo parameter, int parameterIndex, FieldBuilder serializer)
        {
            ilGenerator.EmitLoadArg(0);
            ilGenerator.Emit(OpCodes.Ldfld, serializer);
            ilGenerator.Emit(OpCodes.Ldloc_3);
            ilGenerator.EmitLoadArg(parameterIndex + 1);
            ilGenerator.Emit(OpCodes.Callvirt, _serializeMethod);
        }

        private bool TryToWriteValue(ILGenerator ilGenerator, ParameterInfo parameter, int parameterIndex)
        {
            var method = typeof(JsonTextWriter).GetRuntimeMethods().FirstOrDefault(m =>
            {
                if (m.Name != "WriteValue")
                {
                    return false;
                }

                var parameters = m.GetParameters();

                return parameters.Length == 1 && parameters[0].ParameterType == parameter.ParameterType;
            });

            if (method != null)
            {
                ilGenerator.Emit(OpCodes.Ldloc_3);
                ilGenerator.EmitLoadArg(parameterIndex + 1);
                ilGenerator.Emit(OpCodes.Callvirt, method);

                return true;
            }

            return false;
        }

        private void EmitJsonWriter(ILGenerator ilGenerator)
        {
            var jsonConstructor = typeof(JsonTextWriter).GetTypeInfo().DeclaredConstructors.First(c =>
            {
                var parameters = c.GetParameters();

                return parameters.Length == 1 && parameters[0].ParameterType == typeof(TextWriter);
            });

            ilGenerator.Emit(OpCodes.Ldloc_2);
            ilGenerator.Emit(OpCodes.Newobj, jsonConstructor);
            ilGenerator.Emit(OpCodes.Stloc_3);
            ilGenerator.BeginExceptionBlock();
        }

        private void EmitStreamWriter(ILGenerator ilGenerator)
        {
            var streamWriterConstructor = typeof(StreamWriter).GetTypeInfo().DeclaredConstructors.First(c =>
            {
                var parameters = c.GetParameters();

                return parameters.Length == 1 && parameters[0].ParameterType == typeof(Stream);
            });

            ilGenerator.Emit(OpCodes.Ldloc_1);
            ilGenerator.Emit(OpCodes.Newobj, streamWriterConstructor);
            ilGenerator.Emit(OpCodes.Stloc_2);
            ilGenerator.BeginExceptionBlock();
        }

        private void EmitMemoryStream(ILGenerator ilGenerator)
        {
            ilGenerator.Emit(OpCodes.Newobj, typeof(MemoryStream).GetTypeInfo().DeclaredConstructors.First(c => c.GetParameters().Length == 0));
            ilGenerator.Emit(OpCodes.Stloc_1);
            ilGenerator.BeginExceptionBlock();
        }

        private static MethodInfo _serializeMethod =
            typeof(JsonSerializer).GetMethod("Serialize", new[] { typeof(JsonTextWriter), typeof(object) });

        private static MethodInfo _writeParam = typeof(JsonTextWriter).GetMethod("WritePropertyName",
            new[] { typeof(string), typeof(bool) });
    }
}
