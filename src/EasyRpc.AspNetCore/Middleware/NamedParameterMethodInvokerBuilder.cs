using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading.Tasks;
using EasyRpc.AspNetCore.Messages;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;

namespace EasyRpc.AspNetCore.Middleware
{
    public interface INamedParameterMethodInvokerBuilder
    {
        InvokeMethodByNamedParameters BuildInvokeMethodByNamedParameters(MethodInfo methodInfo);
    }

    public class NamedParameterMethodInvokerBuilder : MethodInvokerBuilder, INamedParameterMethodInvokerBuilder
    {
        public InvokeMethodByNamedParameters BuildInvokeMethodByNamedParameters(MethodInfo methodInfo)
        {
            DynamicMethod dynamicMethod = new DynamicMethod(string.Empty,
                                                             typeof(Task<ResponseMessage>),
                                                             new[] { typeof(string), typeof(string), typeof(object), typeof(IDictionary<string, object>), typeof(HttpContext) },
                                                             typeof(MethodInvokerBuilder).GetTypeInfo().Module);

            var ilGenerator = dynamicMethod.GetILGenerator();

            GenerateMethodInvoke(methodInfo, ilGenerator);

            return (InvokeMethodByNamedParameters)dynamicMethod.CreateDelegate(typeof(InvokeMethodByNamedParameters));
        }

        private void GenerateMethodInvoke(MethodInfo methodInfo, ILGenerator ilGenerator)
        {
            ilGenerator.DeclareLocal(methodInfo.DeclaringType);
            ilGenerator.Emit(OpCodes.Ldarg_2);
            ilGenerator.Emit(OpCodes.Castclass, methodInfo.DeclaringType);
            
            foreach (var parameter in methodInfo.GetParameters())
            {
                var fromServices = parameter.GetCustomAttributes<FromServicesAttribute>();

                if (fromServices.Any())
                {
                    GenerateIlForFromServices(parameter, ilGenerator);
                }
                else
                {
                    GenerateIlForParameter(parameter, ilGenerator);
                }
            }

            ilGenerator.EmitMethodCall(methodInfo);

            GenerateReturnStatements(methodInfo, ilGenerator);
        }
        
        private void GenerateIlForParameter(ParameterInfo parameter, ILGenerator ilGenerator)
        {
            ilGenerator.Emit(OpCodes.Ldarg_3);

            ilGenerator.Emit(OpCodes.Ldstr, parameter.Name);

            if (parameter.HasDefaultValue)
            {
                if (parameter.ParameterType == typeof(int))
                {
                    ilGenerator.EmitInt((int)parameter.DefaultValue);
                }
                else if (parameter.ParameterType == typeof(string))
                {
                    ilGenerator.Emit(OpCodes.Ldstr, (string)parameter.DefaultValue);
                }
                else if (parameter.ParameterType == typeof(double))
                {
                    ilGenerator.Emit(OpCodes.Ldc_R8, (double)parameter.DefaultValue);
                }
                else
                {
                    ilGenerator.Emit(OpCodes.Initobj, parameter.ParameterType);
                }

                ilGenerator.EmitMethodCall(_getValueFromDictionaryOrDefaultMethodInfo.MakeGenericMethod(parameter.ParameterType));
            }
            else
            {
                ilGenerator.EmitMethodCall(_getValueFromDictionaryMethodInfo.MakeGenericMethod(parameter.ParameterType));
            }
        }

        private static readonly MethodInfo _getValueFromDictionaryMethodInfo =
            typeof(NamedParameterMethodInvokerBuilder).GetMethod("GetValueFromDictionary");

        public static T GetValueFromDictionary<T>(IDictionary<string, object> values, string valueName)
        {
            object value;

            if (!values.TryGetValue(valueName, out value))
            {
                throw new Exception($"Parameter {valueName} is missing");
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

        private static readonly MethodInfo _getValueFromDictionaryOrDefaultMethodInfo =
            typeof(NamedParameterMethodInvokerBuilder).GetMethod("GetValueFromDictionaryOrDefault");

        public static T GetValueFromDictionaryOrDefault<T>(IDictionary<string, object> values, string valueName, T defaultValue)
        {
            object value;

            values.TryGetValue(valueName, out value);

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
    }
}
