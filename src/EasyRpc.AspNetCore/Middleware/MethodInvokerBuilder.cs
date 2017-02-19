using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading.Tasks;
using EasyRpc.AspNetCore.Messages;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json.Linq;

namespace EasyRpc.AspNetCore.Middleware
{
    
    public class MethodInvokerBuilder
    {
        protected void GenerateReturnStatements(MethodInfo methodInfo, ILGenerator ilGenerator)
        {
            ilGenerator.Emit(OpCodes.Ldarg_0);
            ilGenerator.Emit(OpCodes.Ldarg_1);

            var returnType = methodInfo.ReturnType;

            if (returnType == typeof(void))
            {
                var constructor =
                    typeof(ResponseMessage).GetTypeInfo().DeclaredConstructors.First(c => c.GetParameters().Length == 2);

                ilGenerator.Emit(OpCodes.Newobj, constructor);

                ilGenerator.EmitMethodCall(_taskFromResult.MakeGenericMethod(typeof(ResponseMessage)));
            }
            else if (typeof(Task).GetTypeInfo().IsAssignableFrom(returnType.GetTypeInfo()))
            {
                Type genericType;

                if (IsTaskResultType(returnType, out genericType))
                {
                    var openMethod =
                        typeof(MethodInvokerBuilder).GetRuntimeMethods()
                            .First(m => m.Name == "CreateAsyncResponseGeneric");

                    ilGenerator.EmitMethodCall(openMethod.MakeGenericMethod(genericType));
                }
                else
                {
                    ilGenerator.EmitMethodCall(typeof(MethodInvokerBuilder).GetRuntimeMethod("CreateAsyncResponse", new Type[] { typeof(Task), typeof(string), typeof(string) }));
                }
            }
            else
            {
                var responseType = typeof(ResponseMessage<>).MakeGenericType(returnType);

                var constructor = responseType.GetConstructors().First(c => c.GetParameters().Length == 3);

                ilGenerator.Emit(OpCodes.Newobj, constructor);

                ilGenerator.EmitMethodCall(_taskFromResult.MakeGenericMethod(typeof(ResponseMessage)));
            }

            ilGenerator.Emit(OpCodes.Ret);
        }
        
        public static async Task<ResponseMessage> CreateAsyncResponse(Task result, string version, string id)
        {
            await result;

            return new ResponseMessage(version, id);
        }

        public static async Task<ResponseMessage> CreateAsyncResponseGeneric<T>(Task<T> result, string version, string id)
        {
            var value = await result;

            return new ResponseMessage<T>(value, version, id);
        }

        private bool IsTaskResultType(Type type, out Type resultType)
        {
            if (type.IsConstructedGenericType && type.GetGenericTypeDefinition() == typeof(Task<>))
            {
                resultType = type.GenericTypeArguments[0];

                return true;
            }

            if (type.GetTypeInfo().BaseType != null && type.GetTypeInfo().BaseType != typeof(object))
            {
                return IsTaskResultType(type.GetTypeInfo().BaseType, out resultType);
            }

            resultType = null;

            return false;
        }

        private static readonly MethodInfo _taskFromResult = typeof(Task).GetRuntimeMethods().Single(m => m.Name == "FromResult");
    }
}
