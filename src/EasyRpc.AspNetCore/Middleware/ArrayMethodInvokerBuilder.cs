using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading.Tasks;
using EasyRpc.AspNetCore.Messages;

namespace EasyRpc.AspNetCore.Middleware
{
    public interface IArrayMethodInvokerBuilder
    {
        InvokeMethodWithArray CreateMethodInvoker(MethodInfo method, bool allowCompression);
    }
    public class ArrayMethodInvokerBuilder : IArrayMethodInvokerBuilder
    {
        public InvokeMethodWithArray CreateMethodInvoker(MethodInfo method, bool allowCompression)
        {
            DynamicMethod dynamicMethod = new DynamicMethod(string.Empty,
                typeof(Task<ResponseMessage>),
                new[] { typeof(object), typeof(object[]), typeof(string), typeof(string) },
                typeof(ArrayMethodInvokerBuilder).GetTypeInfo().Module);

            var ilGenerator = dynamicMethod.GetILGenerator();

            GenerateMethod(method, ilGenerator, allowCompression);

            return (InvokeMethodWithArray)dynamicMethod.CreateDelegate(typeof(InvokeMethodWithArray));
        }

        private void GenerateMethod(MethodInfo methodInfo, ILGenerator ilGenerator, bool allowCompression)
        {
            ilGenerator.DeclareLocal(methodInfo.DeclaringType);
            ilGenerator.Emit(OpCodes.Ldarg_0);
            ilGenerator.Emit(OpCodes.Castclass, methodInfo.DeclaringType);

            var parameters = methodInfo.GetParameters();

            for (int i = 0; i < parameters.Length; i++)
            {
                var parameter = parameters[i];
                ilGenerator.Emit(OpCodes.Ldarg_1);
                ilGenerator.EmitInt(i);
                ilGenerator.Emit(OpCodes.Ldelem_Ref);

                ilGenerator.Emit(!parameter.ParameterType.IsByRef ? OpCodes.Unbox_Any : OpCodes.Castclass,
                    parameter.ParameterType);
            }

            ilGenerator.Emit(OpCodes.Callvirt, methodInfo);

            if (allowCompression &&
                methodInfo.ReturnType != typeof(ResponseMessage) &&
                methodInfo.ReturnType != typeof(void) &&
                !methodInfo.ReturnType.GetTypeInfo().IsPrimitive)
            {
                GenerateReturnStatementWithCompression(methodInfo, ilGenerator);
            }
            else
            {
                GenerateReturnStatement(methodInfo, ilGenerator);
            }
        }

        private void GenerateReturnStatementWithCompression(MethodInfo methodInfo, ILGenerator ilGenerator)
        {
            ilGenerator.Emit(OpCodes.Ldarg_2);
            ilGenerator.Emit(OpCodes.Ldarg_3);

            var returnType = methodInfo.ReturnType;

            if (typeof(Task).GetTypeInfo().IsAssignableFrom(returnType.GetTypeInfo()))
            {
                EmitCallForCompressedTask(ilGenerator, returnType);
            }
            else if (returnType == typeof(string))
            {
                ilGenerator.EmitMethodCall(typeof(ArrayMethodInvokerBuilder).GetMethod(nameof(CreateResponseStringCompress)));
            }
            else if (IsICollection(returnType, out var itemType))
            {
                var openMethod = typeof(ArrayMethodInvokerBuilder).GetMethod(nameof(CreateResponseCollectionCompress));

                ilGenerator.EmitMethodCall(openMethod.MakeGenericMethod(itemType));
            }
            else
            {
                var openMethod =
                    typeof(ArrayMethodInvokerBuilder).GetRuntimeMethods()
                        .First(m => m.Name == nameof(CreateResponseCompress));

                ilGenerator.EmitMethodCall(openMethod.MakeGenericMethod(returnType));
            }

            ilGenerator.Emit(OpCodes.Ret);
        }

        private static void EmitCallForCompressedTask(ILGenerator ilGenerator, Type returnType)
        {
            if (IsTaskResultType(returnType, out var genericType))
            {
                if (genericType == typeof(string))
                {
                    ilGenerator.EmitMethodCall(
                        typeof(ArrayMethodInvokerBuilder).GetMethod(nameof(CreateAsyncResponseStringCompress)));
                }
                else if (typeof(ResponseMessage).IsAssignableFrom(genericType))
                {
                    ilGenerator.EmitMethodCall(
                        typeof(ArrayMethodInvokerBuilder).GetMethod(nameof(CreateAsyncResponseMessage)));
                }
                else if (IsICollection(genericType, out var itemType))
                {
                    var openMethod =
                        typeof(ArrayMethodInvokerBuilder).GetMethod(nameof(CreateAsyncResponseCollectionCompress));

                    ilGenerator.EmitMethodCall(openMethod.MakeGenericMethod(itemType));
                }
                else if (!genericType.GetTypeInfo().IsPrimitive)
                {
                    var openMethod =
                        typeof(ArrayMethodInvokerBuilder).GetMethod(nameof(CreateAsyncResponseGenericCompress));

                    ilGenerator.EmitMethodCall(openMethod.MakeGenericMethod(genericType));
                }
                else
                {
                    var openMethod =
                        typeof(ArrayMethodInvokerBuilder).GetMethod(nameof(CreateAsyncResponseGeneric));

                    ilGenerator.EmitMethodCall(openMethod.MakeGenericMethod(genericType));
                }
            }
            else
            {
                ilGenerator.EmitMethodCall(typeof(ArrayMethodInvokerBuilder).GetMethod(nameof(CreateAsyncResponse)));
            }
        }

        private static bool IsICollection(Type type, out Type itemType)
        {
            if (type.IsConstructedGenericType)
            {
                var openGenericType = type.GetGenericTypeDefinition();

                if (openGenericType == typeof(ICollection<>))
                {
                    itemType = openGenericType.GetTypeInfo().GenericTypeArguments[0];

                    return true;
                }
            }

            itemType = null;

            foreach (var implementedInterface in type.GetTypeInfo().ImplementedInterfaces)
            {
                if (implementedInterface.IsConstructedGenericType &&
                    implementedInterface.GetGenericTypeDefinition() == typeof(ICollection<>))
                {
                    itemType = implementedInterface.GetTypeInfo().GenericTypeArguments[0];
                    return true;
                }
            }

            return false;
        }

        private static void GenerateReturnStatement(MethodInfo methodInfo, ILGenerator ilGenerator)
        {
            ilGenerator.Emit(OpCodes.Ldarg_2);
            ilGenerator.Emit(OpCodes.Ldarg_3);

            var returnType = methodInfo.ReturnType;

            if (returnType == typeof(void))
            {
                var constructor =
                    typeof(EmptyResponseMessage).GetTypeInfo().DeclaredConstructors.First(c => c.GetParameters().Length == 2);

                ilGenerator.Emit(OpCodes.Newobj, constructor);

                ilGenerator.EmitMethodCall(TaskFromResult.MakeGenericMethod(typeof(ResponseMessage)));
            }
            else if (typeof(Task).GetTypeInfo().IsAssignableFrom(returnType.GetTypeInfo()))
            {
                if (IsTaskResultType(returnType, out var genericType))
                {
                    if (typeof(ResponseMessage).IsAssignableFrom(genericType))
                    {
                        ilGenerator.EmitMethodCall(typeof(ArrayMethodInvokerBuilder).GetMethod(nameof(CreateAsyncResponseMessage)));
                    }
                    else
                    {
                        var openMethod =
                            typeof(ArrayMethodInvokerBuilder).GetMethod(nameof(CreateAsyncResponseGeneric));

                        ilGenerator.EmitMethodCall(openMethod.MakeGenericMethod(genericType));
                    }
                }
                else
                {
                    ilGenerator.EmitMethodCall(typeof(ArrayMethodInvokerBuilder).GetMethod(nameof(CreateAsyncResponse)));
                }
            }
            else if (typeof(ResponseMessage).GetTypeInfo().IsAssignableFrom(returnType.GetTypeInfo()))
            {
                ilGenerator.EmitMethodCall(typeof(ArrayMethodInvokerBuilder).GetMethod(nameof(CreateResponseMessage)));
            }
            else
            {
                var responseType = typeof(ResponseMessage<>).MakeGenericType(returnType);

                var constructor = responseType.GetConstructors().First(c => c.GetParameters().Length == 3);

                ilGenerator.Emit(OpCodes.Newobj, constructor);

                ilGenerator.EmitMethodCall(TaskFromResult.MakeGenericMethod(typeof(ResponseMessage)));
            }

            ilGenerator.Emit(OpCodes.Ret);
        }

        #region Default response

        public static async Task<ResponseMessage> CreateAsyncResponse(Task result, string version, string id)
        {
            await result;

            return new ResponseMessage(version, id);
        }

        public static async Task<ResponseMessage> CreateAsyncResponseGeneric<T>(Task<T> result, string version, string id)
        {
            return new ResponseMessage<T>(await result, version, id);
        }

        public static Task<ResponseMessage> CreateResponseCompress<T>(T result, string version, string id)
        {
            return Task.FromResult<ResponseMessage>(new ResponseMessage<T>(result, version, id) { CanCompress = result != null });
        }

        public static async Task<ResponseMessage> CreateAsyncResponseGenericCompress<T>(Task<T> result, string version, string id)
        {
            var resultValue = await result;

            return new ResponseMessage<T>(resultValue, version, id) { CanCompress = resultValue != null };
        }

        #endregion

        #region Response Message

        public static Task<ResponseMessage> CreateResponseMessage(ResponseMessage message, string version, string id)
        {
            message.Version = version;
            message.Id = id;

            return Task.FromResult(message);
        }

        public static async Task<ResponseMessage> CreateAsyncResponseMessage(Task<ResponseMessage> result, string version, string id)
        {
            var message = await result;

            message.Version = version;
            message.Id = id;

            return message;
        }
        #endregion

        #region Compress String methods

        public static async Task<ResponseMessage> CreateAsyncResponseStringCompress(Task<string> result, string version, string id)
        {
            var resultString = await result;

            if (resultString != null && resultString.Length > 700)
            {
                return new ResponseMessage<string>(resultString, version, id) { CanCompress = true };
            }

            return new ResponseMessage<string>(resultString, version, id);
        }

        public static Task<ResponseMessage> CreateResponseStringCompress(string result, string version, string id)
        {
            var resultString = result;

            if (resultString != null && resultString.Length > 700)
            {
                return Task.FromResult<ResponseMessage>(new ResponseMessage<string>(resultString, version, id) { CanCompress = true });
            }

            return Task.FromResult<ResponseMessage>(new ResponseMessage<string>(resultString, version, id));
        }

        #endregion

        #region Collection Compress

        public static async Task<ResponseMessage> CreateAsyncResponseCollectionCompress<T>(Task<ICollection<T>> result, string version, string id)
        {
            var resultCollection = await result;

            if (resultCollection != null && resultCollection.Count > 0)
            {
                return new ResponseMessage<ICollection<T>>(resultCollection, version, id) { CanCompress = true };
            }

            return new ResponseMessage<ICollection<T>>(resultCollection, version, id);
        }


        public static Task<ResponseMessage> CreateResponseCollectionCompress<T>(ICollection<T> result, string version, string id)
        {
            var resultCollection = result;

            if (resultCollection != null && resultCollection.Count > 0)
            {
                return Task.FromResult<ResponseMessage>(new ResponseMessage<ICollection<T>>(resultCollection, version, id)
                {
                    CanCompress = true
                });
            }

            return Task.FromResult<ResponseMessage>(new ResponseMessage<ICollection<T>>(resultCollection, version, id));
        }

        #endregion

        private static bool IsTaskResultType(Type type, out Type resultType)
        {
            while (true)
            {
                if (type.IsConstructedGenericType && type.GetGenericTypeDefinition() == typeof(Task<>))
                {
                    resultType = type.GenericTypeArguments[0];

                    return true;
                }

                if (type.GetTypeInfo().BaseType != null && type.GetTypeInfo().BaseType != typeof(object))
                {
                    type = type.GetTypeInfo().BaseType;

                    continue;
                }

                resultType = null;

                return false;
            }
        }

        private static readonly MethodInfo TaskFromResult = typeof(Task).GetRuntimeMethods().Single(m => m.Name == "FromResult");
    }
}
