using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using EasyRpc.AspNetCore.Middleware;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace EasyRpc.AspNetCore.Converters
{
    public delegate object[] ParamsDeserializer(HttpContext context, RpcJsonReader reader, JsonSerializer serializer);

    public interface IParameterArrayDeserializerBuilder
    {
        ParamsDeserializer BuildDeserializer(IExposedMethodInformation exposedMethod);
    }

    public class ParameterArrayDeserializerBuilder : IParameterArrayDeserializerBuilder
    {
        private readonly IFromServicesManager _fromServicesManager;

        public ParameterArrayDeserializerBuilder(IFromServicesManager fromServicesManager)
        {
            _fromServicesManager = fromServicesManager;
        }

        public ParamsDeserializer BuildDeserializer(IExposedMethodInformation exposedMethod)
        {
            DynamicMethod dynamicMethod = new DynamicMethod(string.Empty,
                typeof(object[]),
                new[] { typeof(HttpContext), typeof(RpcJsonReader), typeof(JsonSerializer) },
                GetType().GetTypeInfo().Module);

            var ilGenerator = dynamicMethod.GetILGenerator();

            GenerateMethod(exposedMethod, ilGenerator);

            return dynamicMethod.CreateDelegate(typeof(ParamsDeserializer)) as ParamsDeserializer;
        }

        protected virtual void GenerateMethod(IExposedMethodInformation exposedMethod, ILGenerator ilGenerator)
        {
            var parameters = exposedMethod.Parameters.ToArray();

            ilGenerator.EmitInt(parameters.Length);

            ilGenerator.Emit(OpCodes.Newarr, typeof(object));

            var parameterIndex = 0;

            for (var index = 0; index < parameters.Length; index++)
            {
                var parameter = parameters[index];

                ilGenerator.Emit(OpCodes.Dup);
                ilGenerator.EmitInt(index);

                if (_fromServicesManager.ParameterIsFromServices(parameter.ParameterInfo))
                {
                    GenerateIlForFromServices(parameter.ParameterInfo, ilGenerator);
                }
                else if (parameter.ParameterType == typeof(HttpContext))
                {
                    GenerateIlForHttpContext(parameter.ParameterInfo, ilGenerator);
                }
                else if (parameter.ParameterType == typeof(IServiceProvider))
                {
                    GenerateIlForServiceProvider(parameter.ParameterInfo, ilGenerator);
                }
                else
                {
                    GenerateIlForParameter(parameter.ParameterInfo, ilGenerator, parameterIndex);
                    parameterIndex++;
                }

                if (!parameter.ParameterType.IsByRef)
                {
                    ilGenerator.Emit(OpCodes.Box, parameter.ParameterType);
                }

                ilGenerator.Emit(OpCodes.Stelem_Ref);
            }

            ilGenerator.Emit(OpCodes.Ret);
        }

        private void GenerateIlForHttpContext(ParameterInfo parameter, ILGenerator ilGenerator)
        {
            ilGenerator.Emit(OpCodes.Ldarg_0);
        }

        private void GenerateIlForServiceProvider(ParameterInfo parameter, ILGenerator ilGenerator)
        {
            ilGenerator.Emit(OpCodes.Ldarg_0);

            var requestService = typeof(HttpContext).GetTypeInfo().GetProperty("RequestServices");

            ilGenerator.EmitMethodCall(requestService.GetMethod);
        }

        private void GenerateIlForParameter(ParameterInfo parameter, ILGenerator ilGenerator, int parameterIndex)
        {
            if (parameter.ParameterType == typeof(int))
            {
                ilGenerator.Emit(OpCodes.Ldarg_1);

                if (parameter.HasDefaultValue)
                {
                    ilGenerator.EmitInt((int)parameter.DefaultValue);

                    var intValueMethod = GetType().GetMethod("GetIntValueOrDefault");

                    ilGenerator.EmitMethodCall(intValueMethod);
                }
                else
                {
                    ilGenerator.EmitInt(parameterIndex);

                    var intValueMethod = GetType().GetMethod("GetIntValue");

                    ilGenerator.EmitMethodCall(intValueMethod);
                }
            }
            else if (parameter.ParameterType == typeof(string))
            {
                ilGenerator.Emit(OpCodes.Ldarg_1);
                ilGenerator.EmitInt(parameterIndex);
                
                if (parameter.DefaultValue is string defaultParam)
                {
                    ilGenerator.Emit(OpCodes.Ldstr, defaultParam);
                }
                else
                {
                    ilGenerator.Emit(OpCodes.Ldnull);
                }

                ilGenerator.EmitInt(parameter.HasDefaultValue ? 1 : 0);

                var stringValueMethod = GetType().GetMethod("GetStringValue");

                ilGenerator.EmitMethodCall(stringValueMethod);
            }
            else
            {
                ilGenerator.Emit(OpCodes.Ldarg_1);
                ilGenerator.Emit(OpCodes.Ldarg_2);

                var generic = GetType().GetTypeInfo().DeclaredMethods.First(m => m.Name == "GetInstance");

                var closedMethod = generic.MakeGenericMethod(parameter.ParameterType);

                ilGenerator.EmitMethodCall(closedMethod);
            }
        }

        private void GenerateIlForFromServices(ParameterInfo parameter, ILGenerator ilGenerator)
        {
            ilGenerator.Emit(OpCodes.Ldarg_0);

            var method = GetType().GetMethod("GetService");

            var closedMethod = method.MakeGenericMethod(parameter.ParameterType);

            ilGenerator.EmitMethodCall(closedMethod);
        }

        public static T GetService<T>(HttpContext context)
        {
            return (T)context.RequestServices.GetService(typeof(T));
        }

        public static int GetIntValue(RpcJsonReader reader, int index)
        {
            if (reader.TokenType != JsonToken.Integer)
            {
                throw new Exception("Could not get int value for parameter index " + index);
            }

            var returnValue = Convert.ToInt32(reader.Value);

            reader.Read();

            return returnValue;
        }

        public static int GetIntValueOrDefault(RpcJsonReader reader, int defaultValue)
        {
            if (reader.TokenType != JsonToken.Integer)
            {
                return defaultValue;
            }

            var returnValue = (int)reader.Value;

            reader.Read();

            return returnValue;
        }

        public static string GetStringValue(RpcJsonReader reader, int index, string defaultValue, bool hasDefaultValue)
        {
            if (reader.TokenType != JsonToken.String)
            {
                if (hasDefaultValue)
                {
                    return defaultValue;
                }

                throw new Exception("Could not get string value for parameter index " + index);
            }

            var value = (string)reader.Value;

            reader.Read();

            return value;
        }

        public static T GetInstance<T>(RpcJsonReader reader, JsonSerializer serializer)
        {
            var instance = serializer.Deserialize<T>(reader);

            reader.Read();

            return instance;
        }

        private T GetInstanceWithDefault<T>(RpcJsonReader reader, JsonSerializer serializer, T defaultValue)
        {
            if (reader.TokenType == JsonToken.Null)
            {
                reader.Read();

                return defaultValue;
            }

            return GetInstance<T>(reader, serializer);
        }
    }
}
