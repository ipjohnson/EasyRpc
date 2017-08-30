using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using Newtonsoft.Json.Linq;

namespace EasyRpc.AspNetCore.Middleware
{
    public interface IOrderedParameterToArrayDelegateProvider
    {
        OrderedParametersToArray CreateDelegate(MethodInfo method);
    }

    public class OrderedParameterToArrayDelegateProvider : BaseDelegateProvider<object[],OrderedParametersToArray>, IOrderedParameterToArrayDelegateProvider
    {
        protected override void GenerateIlForParameter(ParameterInfo parameter, ILGenerator ilGenerator, int index)
        {
            ilGenerator.Emit(OpCodes.Ldarg_0);

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

                ilGenerator.EmitMethodCall(GetValueOrDefaultFromArrayMethodInfo.MakeGenericMethod(parameter.ParameterType));
            }
            else
            {
                ilGenerator.EmitMethodCall(GetValueFromArrayMethodInfo.MakeGenericMethod(parameter.ParameterType));
            }
        }

        private static readonly MethodInfo GetValueFromArrayMethodInfo =
            typeof(OrderedParameterToArrayDelegateProvider).GetRuntimeMethod("GetValueFromArray", new[] { typeof(Object[]), typeof(int) });

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
            typeof(OrderedParameterToArrayDelegateProvider).GetRuntimeMethods().First(m => m.Name == "GetValueOrDefaultFromArray");

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
        
    }
}
