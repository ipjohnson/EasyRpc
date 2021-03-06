﻿using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using EasyRpc.AspNetCore.EndPoints;

namespace EasyRpc.AspNetCore.ModelBinding.InternalRouting
{
    public interface IInternalRoutingParameterBinder : IUrlParameterBinder
    {
    }

    public class InternalRoutingParameterBinder : IInternalRoutingParameterBinder
    {

        public void BindUrlParameter(RequestExecutionContext context, EndPointMethodConfiguration configuration,
            IRpcParameterInfo parameter, IRequestParameters parameterContext, ref int currentIndex, in ReadOnlySpan<char> pathSpan)
        {
            object parameterValue;

            if (parameter.ParamType == typeof(int))
            {
                parameterValue = ParseInt(context, configuration, parameter, parameterContext, ref currentIndex, pathSpan);
            }
            else if (parameter.ParamType == typeof(string))
            {
                if (parameter.TokenStopCharacter.HasValue)
                {
                    parameterValue = ParseString(context, configuration, parameter, parameterContext, ref currentIndex,
                        pathSpan);
                }
                else
                {
                    parameterValue = ParseGreedyString(ref currentIndex, pathSpan);
                }
            }
            else if (parameter.ParamType == typeof(double))
            {
                parameterValue = ParseDouble(context, configuration, parameter, parameterContext, ref currentIndex, pathSpan);
            }
            else if (parameter.ParamType == typeof(decimal))
            {
                parameterValue = ParseDecimal(context, configuration, parameter, parameterContext, ref currentIndex, pathSpan);
            }
            else if (parameter.ParamType == typeof(Guid))
            {
                parameterValue = ParseGuid(context, configuration, parameter, parameterContext, ref currentIndex,
                    pathSpan);
            }
            else if (parameter.ParamType == typeof(long))
            {
                parameterValue = ParseLong(context, configuration, parameter, parameterContext, ref currentIndex,
                    pathSpan);
            }
            else
            {
                HandleUnknownParameterType(context, configuration, parameter, parameterContext, ref currentIndex, pathSpan);
                return;
            }

            if (parameterValue != null)
            {
                parameterContext[parameter.Position] = parameterValue;
            }
        }

        private string ParseGreedyString(ref int currentIndex, in ReadOnlySpan<char> pathSpan)
        {
            var slice = pathSpan.Slice(currentIndex, pathSpan.Length - currentIndex);

            return new string(slice);
        }

        private object ParseLong(RequestExecutionContext context, EndPointMethodConfiguration configuration, IRpcParameterInfo parameter, IRequestParameters parameterContext, ref int currentIndex, in ReadOnlySpan<char> pathSpan)
        {
            long currentLongValue = 0;
            var foundValue = false;

            while (pathSpan.Length > currentIndex)
            {
                var currentChar = pathSpan[currentIndex];
                currentIndex++;

                if (char.IsDigit(currentChar))
                {
                    currentLongValue = (currentLongValue * 10) + (currentChar - '0');

                    foundValue = true;
                }
                else if (currentChar == parameter.TokenStopCharacter)
                {
                    return currentLongValue;
                }
                else if (parameter.HasDefaultValue)
                {
                    return parameter.DefaultValue;
                }
                else
                {
                    return currentLongValue;
                }
            }

            if (foundValue)
            {
                return currentLongValue;
            }

            return HandleParameterNotFound(context, configuration, parameter, parameterContext, currentIndex, pathSpan);
        }

        private object ParseGuid(RequestExecutionContext context, EndPointMethodConfiguration configuration, IRpcParameterInfo parameter, IRequestParameters parameterContext, ref int currentIndex, in ReadOnlySpan<char> pathSpan)
        {
            var guidString = ParseString(context, configuration, parameter, parameterContext, ref currentIndex, pathSpan);

            if (Guid.TryParse(guidString, out var decimalValue))
            {
                return decimalValue;
            }

            if (parameter.HasDefaultValue)
            {
                return parameter.DefaultValue;
            }

            return HandleParsingError(context, configuration, parameter, parameterContext, ref currentIndex, pathSpan);
        }

        protected virtual object ParseDecimal(RequestExecutionContext context, EndPointMethodConfiguration configuration, IRpcParameterInfo parameter, IRequestParameters parameterContext, ref int currentIndex, in ReadOnlySpan<char> pathSpan)
        {
            var decimalString = ParseString(context, configuration, parameter, parameterContext, ref currentIndex, pathSpan);

            if (decimal.TryParse(decimalString, out var decimalValue))
            {
                return decimalValue;
            }

            if (parameter.HasDefaultValue)
            {
                return parameter.DefaultValue;
            }

            return HandleParsingError(context, configuration, parameter, parameterContext, ref currentIndex, pathSpan);
        }

        protected virtual object ParseDouble(RequestExecutionContext context, EndPointMethodConfiguration configuration, IRpcParameterInfo parameter, IRequestParameters parameterContext, ref int currentIndex, in ReadOnlySpan<char> pathSpan)
        {
            var doubleString = ParseString(context, configuration, parameter, parameterContext, ref currentIndex, pathSpan);

            if (double.TryParse(doubleString, out var doubleValue))
            {
                return doubleValue;
            }

            if (parameter.HasDefaultValue)
            {
                return parameter.DefaultValue;
            }

            return HandleParsingError(context, configuration, parameter, parameterContext, ref currentIndex, pathSpan);
        }

        private object HandleParsingError(RequestExecutionContext context, EndPointMethodConfiguration configuration, IRpcParameterInfo parameter, IRequestParameters parameterContext, ref int currentIndex, in ReadOnlySpan<char> pathSpan)
        {

            return null;
        }

        protected virtual string ParseString(RequestExecutionContext context, EndPointMethodConfiguration configuration, IRpcParameterInfo parameter, IRequestParameters parameterContext, ref int currentIndex, in ReadOnlySpan<char> pathSpan)
        {
            var stringStart = currentIndex;
            var requireDecoding = false;

            while (pathSpan.Length > currentIndex)
            {
                var currentChar = pathSpan[currentIndex];
                currentIndex++;

                if (currentChar == parameter.TokenStopCharacter)
                {
                    break;
                }

                if (currentChar == '%')
                {
                    requireDecoding = true;
                }
            }

            string returnString;

            if (currentIndex < pathSpan.Length)
            {
                var slice = pathSpan.Slice(stringStart, (currentIndex - 1)- stringStart);

                returnString = new string(slice);
            }
            else
            {
                returnString = new string(pathSpan.Slice(stringStart));
            }

            return requireDecoding ? WebUtility.UrlDecode(returnString) : returnString;
        }

        protected virtual object ParseInt(RequestExecutionContext context, EndPointMethodConfiguration configuration, IRpcParameterInfo parameter, IRequestParameters parameterContext, ref int currentIndex, in ReadOnlySpan<char> pathSpan)
        {
            int currentIntValue = 0;
            var foundValue = false;

            while (pathSpan.Length > currentIndex)
            {
                var currentChar = pathSpan[currentIndex];
                currentIndex++;

                if (char.IsDigit(currentChar))
                {
                    currentIntValue = (currentIntValue * 10) + (currentChar - '0');

                    foundValue = true;
                }
                else if (currentChar == parameter.TokenStopCharacter)
                {
                    return currentIntValue;
                }
                else if (parameter.HasDefaultValue)
                {
                    return parameter.DefaultValue;
                }
                else
                {
                    return currentIntValue;
                }
            }

            if (foundValue)
            {
                return currentIntValue;
            }

            return HandleParameterNotFound(context, configuration, parameter, parameterContext, currentIndex, pathSpan);
        }

        protected virtual void HandleUnknownParameterType(RequestExecutionContext requestExecutionContext,
            EndPointMethodConfiguration configuration, IRpcParameterInfo parameter, IRequestParameters parameterContext,
            ref int currentIndex, in ReadOnlySpan<char> pathSpan)
        {
            throw new Exception($"Can't map parameter {pathSpan.ToString()} to type {parameter.ParamType.FullName}");
        }

        protected virtual object HandleParameterNotFound(RequestExecutionContext context, EndPointMethodConfiguration configuration, IRpcParameterInfo parameter, IRequestParameters parameterContext, in int currentIndex, in ReadOnlySpan<char> pathSpan)
        {
            if (parameter.HasDefaultValue)
            {
                return parameter.DefaultValue;
            }

            if (parameter.ParamType.IsValueType)
            {
                return Activator.CreateInstance(parameter.ParamType);
            }

            return null;
        }
    }
}
