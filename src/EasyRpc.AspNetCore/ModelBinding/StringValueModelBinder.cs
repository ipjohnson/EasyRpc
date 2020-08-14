using System;
using System.Collections.Generic;
using System.Text;

namespace EasyRpc.AspNetCore.ModelBinding
{
    public interface IStringValueModelBinder
    {
        object ConvertString(IRpcParameterInfo parameter, string stringValue);
    }

    public class StringValueModelBinder : IStringValueModelBinder
    {
        public object ConvertString(IRpcParameterInfo parameter, string stringValue)
        {
            var parameterType = parameter.ParamType;

            if (parameterType == typeof(string))
            {
                return stringValue;
            }

            if (parameterType == typeof(int) || 
                parameterType == typeof(int?))
            {
                return ParameterIntegerValue(parameter, stringValue, parameterType);
            }

            if (parameterType == typeof(double) ||
                parameterType == typeof(double?))
            {
                return ParameterDoubleValue(parameter, stringValue, parameterType);
            }
            
            if (parameterType == typeof(decimal) ||
                parameterType == typeof(decimal?))
            {
                return ParameterDecimalValue(parameter, stringValue, parameterType);
            }

            if (parameterType == typeof(bool) ||
                parameterType == typeof(bool?))
            {
                return ParameterBooleanValue(parameter, stringValue, parameterType);
            }

            return Convert.ChangeType(stringValue, parameterType);
        }

        protected virtual object ParameterBooleanValue(IRpcParameterInfo parameter, string stringValue, Type parameterType)
        {
            if (bool.TryParse(stringValue, out var intValue))
            {
                return intValue;
            }

            if (parameter.HasDefaultValue)
            {
                return parameter.DefaultValue;
            }

            if (parameterType == typeof(bool?))
            {
                return null;
            }

            throw new Exception($"Could not convert parameter {parameter.Name} value {stringValue} to bool");
        }

        protected virtual object ParameterDoubleValue(IRpcParameterInfo parameter, string stringValue, Type parameterType)
        {
            if (double.TryParse(stringValue, out var intValue))
            {
                return intValue;
            }

            if (parameter.HasDefaultValue)
            {
                return parameter.DefaultValue;
            }

            if (parameterType == typeof(double?))
            {
                return null;
            }

            throw new Exception($"Could not convert parameter {parameter.Name} value {stringValue} to double");
        }

        protected virtual object ParameterDecimalValue(IRpcParameterInfo parameter, string stringValue, Type parameterType)
        {
            if (decimal.TryParse(stringValue, out var intValue))
            {
                return intValue;
            }

            if (parameter.HasDefaultValue)
            {
                return parameter.DefaultValue;
            }

            if (parameterType == typeof(decimal?))
            {
                return null;
            }

            throw new Exception($"Could not convert parameter {parameter.Name} value {stringValue} to double");
        }

        protected virtual object ParameterIntegerValue(IRpcParameterInfo parameter, string stringValue, Type parameterType)
        {
            if (int.TryParse(stringValue, out var intValue))
            {
                return intValue;
            }

            if (parameter.HasDefaultValue)
            {
                return parameter.DefaultValue;
            }

            if (parameterType == typeof(int?))
            {
                return null;
            }

            throw new Exception($"Could not convert parameter {parameter.Name} value {stringValue} to integer");
        }
    }
}
