using System;
using System.Linq;
using System.Threading;
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
        private readonly IFromServicesManager _fromService;

        public NamedParameterDeserializerBuilder(IFromServicesManager fromService)
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

            public bool CancellationToken { get; set; }
        }
        
        public ParamsDeserializer BuildDeserializer(IExposedMethodInformation exposedMethod)
        {
            var signatureInfo = GetSignatureInfo(exposedMethod);

            return (context, reader,serializer) => DeserializeParameters(context, reader, serializer, signatureInfo);
        }

        private object[] DeserializeParameters(HttpContext context, RpcJsonReader reader, JsonSerializer serializer,
            RpcParameterInfo[] parameters)
        {
            var returnParameters = new object[parameters.Length];

            for (var i = 0; i < parameters.Length; i++)
            {
                var parameter = parameters[i];

                if (parameter.HasDefaultValue)
                {
                    returnParameters[i] = parameter.DefaultValue;
                }
                else if(parameter.HttpContext)
                {
                    returnParameters[i] = context;
                }
                else if(parameter.ServiceProvider)
                {
                    returnParameters[i] = context.RequestServices;
                }
                else if (parameter.FromServices)
                {
                    returnParameters[i] = context.RequestServices.GetService(parameter.ParameterType);
                }
                else if (parameter.CancellationToken)
                {
                    returnParameters[i] = context.RequestAborted;
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

        private RpcParameterInfo[] GetSignatureInfo(IExposedMethodInformation exposedMethodInformation)
        { 
            return exposedMethodInformation.Parameters.Select(parameterInfo =>
            {
                var fromServices = false;
                var httpContext = parameterInfo.ParameterType == typeof(HttpContext);
                var serviceProvider = parameterInfo.ParameterType == typeof(IServiceProvider);

                if (!httpContext && !serviceProvider)
                {
                    fromServices = _fromService.ParameterIsFromServices(parameterInfo.ParameterInfo);
                }

                return new RpcParameterInfo
                {
                    ParameterIndex = parameterInfo.Position,
                    ParameterName = parameterInfo.Name,
                    ParameterType = parameterInfo.ParameterType,
                    DefaultValue = parameterInfo.DefaultValue,
                    HasDefaultValue = parameterInfo.HasDefaultValue,
                    FromServices = fromServices,
                    HttpContext = httpContext,
                    ServiceProvider = serviceProvider,
                    CancellationToken = parameterInfo.ParameterType == typeof(CancellationToken)
                };
            }).ToArray();
        }
    }
}
