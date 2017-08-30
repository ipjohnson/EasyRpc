using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using Newtonsoft.Json.Linq;

namespace EasyRpc.AspNetCore.Middleware
{
    public interface INamedParameterToArrayDelegateProvider
    {
        NamedParametersToArray CreateDelegate(MethodInfo method);
    }

    public class NamedParameterToArrayDelegateProvider : BaseDelegateProvider<IDictionary<string,object>, NamedParametersToArray>, INamedParameterToArrayDelegateProvider
    {
        protected override void GenerateIlForParameter(ParameterInfo parameter, ILGenerator ilGenerator, int parameterIndex)
        {
            ilGenerator.Emit(OpCodes.Ldarg_0);

            ilGenerator.Emit(OpCodes.Ldstr, parameter.Name);

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
                        ilGenerator.Emit(OpCodes.Ldstr, (string)parameter.DefaultValue);
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

                ilGenerator.EmitMethodCall(GetValueFromDictionaryOrDefaultMethodInfo.MakeGenericMethod(parameter.ParameterType));
            }
            else
            {
                ilGenerator.EmitMethodCall(GetValueFromDictionaryMethodInfo.MakeGenericMethod(parameter.ParameterType));
            }
        }

        private static readonly MethodInfo GetValueFromDictionaryMethodInfo =
            typeof(NamedParameterToArrayDelegateProvider).GetMethod("GetValueFromDictionary");

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

        private static readonly MethodInfo GetValueFromDictionaryOrDefaultMethodInfo =
            typeof(NamedParameterToArrayDelegateProvider).GetMethod("GetValueFromDictionaryOrDefault");

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
