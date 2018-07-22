using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using EasyRpc.AspNetCore.Middleware;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace EasyRpc.AspNetCore.Converters
{
    public delegate object[] ParamsDeserializer(RpcJsonReader reader, JsonSerializer serializer);


    public interface IParameterArrayDeserializerBuilder
    {
        ParamsDeserializer BuildDeserializer(ExposedMethodInformation exposedMethod);
    }

    public class ParameterArrayDeserializerBuilder : IParameterArrayDeserializerBuilder
    {
        public ParamsDeserializer BuildDeserializer(ExposedMethodInformation exposedMethod)
        {
            DynamicMethod dynamicMethod = new DynamicMethod(string.Empty,
                typeof(object[]),
                new[] { typeof(RpcJsonReader), typeof(JsonSerializer) },
                GetType().GetTypeInfo().Module);

            var ilGenerator = dynamicMethod.GetILGenerator();

            GenerateMethod(exposedMethod, ilGenerator);

            return dynamicMethod.CreateDelegate(typeof(ParamsDeserializer)) as ParamsDeserializer;
        }

        protected virtual void GenerateMethod(ExposedMethodInformation exposedMethod, ILGenerator ilGenerator)
        {
            var parameters = exposedMethod.MethodInfo.GetParameters();

            ilGenerator.EmitInt(parameters.Length);

            ilGenerator.Emit(OpCodes.Newarr, typeof(object));

            var parameterIndex = 0;

            for (var index = 0; index < parameters.Length; index++)
            {
                var parameter = parameters[index];

                ilGenerator.Emit(OpCodes.Dup);
                ilGenerator.EmitInt(index);

                var fromServices = parameter.GetCustomAttributes().Any(a => a.GetType().Name == "FromServicesAttribute");

                if (fromServices)
                {
                    GenerateIlForFromServices(parameter, ilGenerator);
                }
                else
                {
                    GenerateIlForParameter(parameter, ilGenerator, parameterIndex);
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

        private void GenerateIlForParameter(ParameterInfo parameter, ILGenerator ilGenerator, int parameterIndex)
        {
            if (parameter.ParameterType == typeof(int))
            {
                ilGenerator.Emit(OpCodes.Ldarg_0);

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
                ilGenerator.Emit(OpCodes.Ldarg_0);
                ilGenerator.EmitInt(parameterIndex);

                if (parameter.DefaultValue is string defaultParam)
                {
                    ilGenerator.Emit(OpCodes.Ldstr, defaultParam);
                }
                else
                {
                    ilGenerator.Emit(OpCodes.Ldnull);
                }

                var stringValueMethod = GetType().GetMethod("GetStringValue");

                ilGenerator.EmitMethodCall(stringValueMethod);
            }
            else
            {
                ilGenerator.Emit(OpCodes.Ldarg_0);
                ilGenerator.Emit(OpCodes.Ldarg_1);

                var generic = GetType().GetTypeInfo().DeclaredMethods.First(m => m.Name == "GetInstance");

                var closedMethod = generic.MakeGenericMethod(parameter.ParameterType);

                ilGenerator.EmitMethodCall(closedMethod);
            }
        }

        private void GenerateIlForFromServices(ParameterInfo parameter, ILGenerator ilGenerator)
        {
            // todo
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

        public static string GetStringValue(RpcJsonReader reader, int index, string defaultValue)
        {
            if (reader.TokenType != JsonToken.String)
            {
                if (defaultValue != null)
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
            return serializer.Deserialize<T>(reader);
        }

        private T GetInstanceWithDefault<T>(RpcJsonReader reader, JsonSerializer serializer, T defaultValue)
        {
            if (reader.TokenType == JsonToken.Null)
            {
                reader.Read();

                return defaultValue;
            }

            return serializer.Deserialize<T>(reader);
        }
    }
}
