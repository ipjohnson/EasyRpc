using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading.Tasks;
using EasyRpc.AspNetCore.Messages;

namespace EasyRpc.AspNetCore.Middleware
{
    public interface IArrayMethodInvokerBuilder
    {
        InvokeMethodWithArray CreateMethodInvoker(MethodInfo method);
    }
    public class ArrayMethodInvokerBuilder : IArrayMethodInvokerBuilder
    {
        public InvokeMethodWithArray CreateMethodInvoker(MethodInfo method)
        {
            DynamicMethod dynamicMethod = new DynamicMethod(string.Empty,
                typeof(Task<ResponseMessage>),
                new[] { typeof(object), typeof(object[]), typeof(string), typeof(string) },
                typeof(ArrayMethodInvokerBuilder).GetTypeInfo().Module);

            var ilGenerator = dynamicMethod.GetILGenerator();

            GenerateMethod(method, ilGenerator);

            return (InvokeMethodWithArray)dynamicMethod.CreateDelegate(typeof(InvokeMethodWithArray));
        }

        private void GenerateMethod(MethodInfo methodInfo, ILGenerator ilGenerator)
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

            GenerateReturnStatements(methodInfo, ilGenerator);
        }

        private void GenerateReturnStatements(MethodInfo methodInfo, ILGenerator ilGenerator)
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
                Type genericType;

                if (IsTaskResultType(returnType, out genericType))
                {
                    var openMethod =
                        typeof(ArrayMethodInvokerBuilder).GetRuntimeMethods()
                            .First(m => m.Name == "CreateAsyncResponseGeneric");

                    ilGenerator.EmitMethodCall(openMethod.MakeGenericMethod(genericType));
                }
                else
                {
                    ilGenerator.EmitMethodCall(typeof(ArrayMethodInvokerBuilder).GetRuntimeMethod("CreateAsyncResponse", new Type[] { typeof(Task), typeof(string), typeof(string) }));
                }
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
