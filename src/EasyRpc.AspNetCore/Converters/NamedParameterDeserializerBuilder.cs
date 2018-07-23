using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using EasyRpc.AspNetCore.Middleware;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace EasyRpc.AspNetCore.Converters
{
    public interface INamedParameterDeserializerBuilder
    {
        ParamsDeserializer BuildDeserializer(IExposedMethodInformation exposedMethod);
    }

    public class NamedParameterDeserializerBuilder : INamedParameterDeserializerBuilder
    {
        private readonly FromServicesManager _fromService;

        public NamedParameterDeserializerBuilder(FromServicesManager fromService)
        {
            _fromService = fromService;
        }

        public class RpcParameterInfo
        {
            public int ParameterIndex { get; set; }

            public string ParameterName { get; set; }

            public Type ParameterType { get; set; }

            public bool HasDefaultValue { get; set; }

            public object DefaultValue { get; set; }

            public bool FromServices { get; set; }

            public bool HttpContext { get; set; }

            public bool ServiceProvider { get; set; }
        }


        public ParamsDeserializer BuildDeserializer(IExposedMethodInformation exposedMethod)
        {
            var signatureInfo = GetSignatureInfo(exposedMethod.MethodInfo);

            return (context, reader,serializer) => DeserializeParameters(context, reader, serializer, signatureInfo);
        }

        private object[] DeserializeParameters(HttpContext context, RpcJsonReader reader, JsonSerializer serializer,
            RpcParameterInfo[] parameters)
        {
            var returnParameters = new object[parameters.Length];

            for (var i = 0; i < parameters.Length; i++)
            {
                if (parameters[i].HasDefaultValue)
                {
                    returnParameters[i] = parameters[i].DefaultValue;
                }

                if (parameters[i].FromServices)
                {
                    
                }
            }

            while (reader.TokenType != JsonToken.EndObject)
            {
                if (reader.TokenType == JsonToken.PropertyName)
                {
                    var propertyName = reader.Value as string;

                    reader.Read();
                    var found = false;

                    for (var i = 0; i < parameters.Length; i++)
                    {
                        if (string.Compare(propertyName, parameters[i].ParameterName,
                                StringComparison.CurrentCultureIgnoreCase) == 0)
                        {
                            returnParameters[i] = serializer.Deserialize(reader, parameters[i].ParameterType);
                            found = true;
                            break;
                        }
                    }

                    if (!found)
                    {
                        serializer.Deserialize(reader);
                    }

                    reader.Read();
                }
                else
                {
                    reader.Read();
                }
            }

            return returnParameters;
        }

        private RpcParameterInfo[] GetSignatureInfo(MethodInfo exposedMethodMethodInfo)
        {
            var index = 0;
            return exposedMethodMethodInfo.GetParameters().Select(parameterInfo => 
                new RpcParameterInfo
                {
                    ParameterIndex = index++,
                    ParameterName = parameterInfo.Name,
                    ParameterType = parameterInfo.ParameterType,
                    DefaultValue = parameterInfo.DefaultValue,
                    HasDefaultValue = parameterInfo.HasDefaultValue
                }).ToArray();
        }
    }
}
