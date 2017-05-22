using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading.Tasks;
using EasyRpc.AspNetCore.Messages;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace EasyRpc.AspNetCore.Middleware
{
    public interface IOrderedParameterMethodInvokeBuilder
    {
        InvokeMethodOrderedParameters BuildInvokeMethodOrderedParameters(MethodInfo methodInfo);
    }

    public class OrderedParameterMethodInvokeBuilder : MethodInvokerBuilder, IOrderedParameterMethodInvokeBuilder
    {
        public InvokeMethodOrderedParameters BuildInvokeMethodOrderedParameters(MethodInfo methodInfo)
        {
            DynamicMethod dynamicMethod = new DynamicMethod(string.Empty,
                                                            typeof(Task<ResponseMessage>),
                                                            new[] { typeof(string), typeof(string), typeof(object), typeof(object[]), typeof(HttpContext) },
                                                            typeof(MethodInvokerBuilder).GetTypeInfo().Module);

            var ilGenerator = dynamicMethod.GetILGenerator();

            GenerateMethodInvoke(methodInfo, ilGenerator);

            return (InvokeMethodOrderedParameters)dynamicMethod.CreateDelegate(typeof(InvokeMethodOrderedParameters));
        }

        private void GenerateMethodInvoke(MethodInfo methodInfo, ILGenerator ilGenerator)
        {
            ilGenerator.DeclareLocal(methodInfo.DeclaringType);
            ilGenerator.Emit(OpCodes.Ldarg_2);
            ilGenerator.Emit(OpCodes.Castclass, methodInfo.DeclaringType);

            var i = 0;
            foreach (var parameter in methodInfo.GetParameters())
            {
                var fromServices = parameter.GetCustomAttributes<FromServicesAttribute>();

                if (fromServices.Any())
                {
                    GenerateIlForFromServices(parameter, ilGenerator);
                }
                else
                {
                    GenerateIlForParameter(parameter, ilGenerator, i);
                    i++;
                }
            }

            ilGenerator.EmitMethodCall(methodInfo);

            GenerateReturnStatements(methodInfo, ilGenerator);
        }


        private static readonly MethodInfo GetValueFromArrayMethodInfo =
            typeof(OrderedParameterMethodInvokeBuilder).GetRuntimeMethod("GetValueFromArray", new[] { typeof(Object[]), typeof(int) });

        public static T GetValueFromArray<T>(object[] values, int index)
        {
            if (values.Length <= index)
            {
                throw new Exception("Not enough parameters provided");
            }

            var value = values[index];

            if (value is T)
            {
                return (T)value;
            }

            if (value is JContainer)
            {
                return ((JContainer)value).ToObject<T>();
            }

            return (T)Convert.ChangeType(value, typeof(T));
        }

        private static readonly MethodInfo GetValueOrDefaultFromArrayMethodInfo =
            typeof(OrderedParameterMethodInvokeBuilder).GetRuntimeMethods().First(m => m.Name == "GetValueOrDefaultFromArray");

        public static T GetValueOrDefaultFromArray<T>(object[] values, int index, T defaultValue)
        {
            if (values.Length <= index)
            {
                return defaultValue;
            }

            var value = values[index];

            if (value == null)
            {
                return defaultValue;
            }

            if (value is T)
            {
                return (T)value;
            }

            if (value is JContainer)
            {
                return ((JContainer)value).ToObject<T>();
            }

            return (T)Convert.ChangeType(value, typeof(T));
        }


        private void GenerateIlForParameter(ParameterInfo parameter, ILGenerator ilGenerator, int index)
        {
            ilGenerator.Emit(OpCodes.Ldarg_3);

            ilGenerator.EmitInt(index);

            if (parameter.HasDefaultValue)
            {
                if (parameter.ParameterType == typeof(int))
                {
                    ilGenerator.EmitInt((int)parameter.DefaultValue);
                }
                else if (parameter.ParameterType == typeof(string))
                {
                    if (parameter.DefaultValue != null)
                    {

                        ilGenerator.Emit(OpCodes.Ldstr, (string) parameter.DefaultValue);
                    }
                    else
                    {
                        ilGenerator.Emit(OpCodes.Ldnull);
                    }
                }
                else if (parameter.ParameterType == typeof(double))
                {
                    ilGenerator.Emit(OpCodes.Ldc_R8, (double)parameter.DefaultValue);
                }
                else
                {
                    ilGenerator.Emit(OpCodes.Initobj, parameter.ParameterType);
                }

                ilGenerator.EmitMethodCall(GetValueOrDefaultFromArrayMethodInfo.MakeGenericMethod(parameter.ParameterType));
            }
            else
            {
                ilGenerator.EmitMethodCall(GetValueFromArrayMethodInfo.MakeGenericMethod(parameter.ParameterType));
            }
        }
    }
}
