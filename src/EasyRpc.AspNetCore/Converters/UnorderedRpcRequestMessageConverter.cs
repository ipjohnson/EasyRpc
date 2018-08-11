using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EasyRpc.AspNetCore.Messages;
using EasyRpc.AspNetCore.Middleware;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace EasyRpc.AspNetCore.Converters
{
    /// <summary>
    /// JsonConverter for RpcRequestMessage that will process message with parameters in any order not just jsonrpc, method, params, id
    /// </summary>
    public class UnorderedRpcRequestMessageConverter : JsonConverter
    {
        protected class ConverterInfo
        {
            public ParamsDeserializer ParameterArrayDeserializer { get; set; }

            public ParamsDeserializer NamedParamsDeserializer { get; set; }

            public IExposedMethodInformation ExposedMethod { get; set; }
        }

        private IParameterArrayDeserializerBuilder _parameterArrayDeserializerBuilder;
        private INamedParameterDeserializerBuilder _namedParameterDeserializerBuilder;
        private IExposeMethodInformationCacheManager _cacheManager;
        private readonly int _serializerId;

        public UnorderedRpcRequestMessageConverter(IParameterArrayDeserializerBuilder parameterArrayDeserializerBuilder, 
                                                   INamedParameterDeserializerBuilder namedParameterDeserializerBuilder, 
                                                   IExposeMethodInformationCacheManager cacheManager, 
                                                   int serializerId)
        {
            _parameterArrayDeserializerBuilder = parameterArrayDeserializerBuilder;
            _namedParameterDeserializerBuilder = namedParameterDeserializerBuilder;
            _cacheManager = cacheManager;
            _serializerId = serializerId;
        }

        /// <summary>Writes the JSON representation of the object.</summary>
        /// <param name="writer">The <see cref="T:Newtonsoft.Json.JsonWriter" /> to write to.</param>
        /// <param name="value">The value.</param>
        /// <param name="serializer">The calling serializer.</param>
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        /// <summary>Reads the JSON representation of the object.</summary>
        /// <param name="reader">The <see cref="T:Newtonsoft.Json.JsonReader" /> to read from.</param>
        /// <param name="objectType">Type of the object.</param>
        /// <param name="existingValue">The existing value of object being read.</param>
        /// <param name="serializer">The calling serializer.</param>
        /// <returns>The object value.</returns>
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var path = ((RpcJsonReader)reader).UrlPath;

            var rpcMessage = new RpcRequestMessage();
            object parametersDictionary = null;

            reader.Read();

            while (reader.TokenType != JsonToken.EndObject)
            {
                if (reader.Value is string parameterName)
                {
                    switch (parameterName)
                    {
                        case "Jsonrpc":
                        case "jsonrpc":
                            ProcessJsonRpc(reader, rpcMessage);
                            break;
                            
                        case "Method":
                        case "method":
                            ProcessMethod(reader, rpcMessage, parametersDictionary, serializer, path);
                            break;

                        case "Params":
                        case "params":
                            ProcessParams(reader, rpcMessage, ref parametersDictionary, serializer, path);
                            break;

                        case "Id":
                        case "id":
                            ProcessId(reader, rpcMessage);
                            break;
                    }
                }

                reader.Read();
            }
            
            if (rpcMessage.ErrorMessage == null && rpcMessage.Parameters == null)
            {
                rpcMessage.Parameters = Array.Empty<object>();
            }

            return rpcMessage;
        }

        private ConverterInfo GetConverter(string urlPath, string method)
        {
            var exposedMethod = _cacheManager.GetExposedMethodInformation(urlPath, method);

            if (exposedMethod == null)
            {
                return null;
            }

            var converter = (ConverterInfo)exposedMethod.GetSerializerData(_serializerId);

            if (converter != null)
            {
                return converter;
            }

            converter = new ConverterInfo { ExposedMethod = exposedMethod };

            exposedMethod.SetSerializerData(_serializerId, converter);

            return converter;
        }


        private void ProcessId(JsonReader reader, RpcRequestMessage rpcMessage)
        {
            rpcMessage.Id = reader.ReadAsString();
        }

        private void ProcessParams(JsonReader reader, RpcRequestMessage rpcMessage,
            ref object parameters, JsonSerializer serializer, string path)
        {
            if (string.IsNullOrEmpty(rpcMessage.Method))
            {
                reader.Read();

                if (reader.TokenType == JsonToken.StartObject)
                {
                    parameters = serializer.Deserialize<IDictionary<string, object>>(reader);
                }
                else
                {
                    parameters = serializer.Deserialize<JArray>(reader);
                }
            }
            else
            {
                var converter = GetConverter(path, rpcMessage.Method);

                if (converter != null)
                {
                    rpcMessage.MethodInformation = converter.ExposedMethod;
                    rpcMessage.Parameters = GetParametersUsingCompiledDelgates((RpcJsonReader)reader, converter, serializer);
                }
                else
                {
                    rpcMessage.ErrorMessage = $"Could not find method {path} {rpcMessage.Method}";
                }
            }
        }

        private object[] GetParametersUsingCompiledDelgates(RpcJsonReader reader, ConverterInfo converter, JsonSerializer serializer)
        {
            if (reader.Read())
            {
                if (reader.TokenType == JsonToken.StartArray)
                {
                    reader.Read();

                    var deserializer = converter.ParameterArrayDeserializer ??
                                       (converter.ParameterArrayDeserializer =
                                           _parameterArrayDeserializerBuilder.BuildDeserializer(converter.ExposedMethod));

                    return deserializer(reader.Context, reader, serializer);
                }

                if (reader.TokenType == JsonToken.StartObject)
                {
                    reader.Read();

                    var deserializer = converter.NamedParamsDeserializer ??
                                       (converter.NamedParamsDeserializer =
                                           _namedParameterDeserializerBuilder.BuildDeserializer(converter.ExposedMethod));

                    return deserializer(reader.Context, reader, serializer);
                }

                if (reader.TokenType == JsonToken.Null)
                {
                    return new object[0];
                }
            }

            throw new Exception("Could not read parameters");
        }


        private void ProcessMethod(JsonReader reader, RpcRequestMessage rpcMessage,
            object parameters, JsonSerializer serializer, string path)
        {
            rpcMessage.Method = reader.ReadAsString();

            if (parameters != null)
            {
                ProcessOutOfOrderParameters(rpcMessage, parameters, path);
            }
        }

        private void ProcessOutOfOrderParameters(RpcRequestMessage rpcMessage, object parameters, string path)
        {
            var converter = GetConverter(path, rpcMessage.Method);

            if (converter == null)
            {
                // todo error message
                return;
            }

            rpcMessage.MethodInformation = converter.ExposedMethod;

            var parameterDefs = converter.ExposedMethod.Parameters.ToArray();

            if (parameterDefs.Length == 0)
            {
                rpcMessage.Parameters = Array.Empty<object>();
                return;
            }

            if (parameters is IDictionary<string, object> parameterDictionary)
            {
                var parameterValues = new object[parameterDefs.Length];

                for (var i = 0; i < parameterValues.Length; i++)
                {
                    var paramInfo = parameterDefs[i];

                    if (parameterDictionary.TryGetValue(paramInfo.Name, out var paramValue))
                    {
                        if (paramValue is JObject jObject)
                        {
                            parameterValues[i] = jObject.ToObject(paramInfo.ParameterType);
                        }
                        else if(paramValue.GetType() == paramInfo.ParameterType)
                        {
                            parameterValues[i] = paramValue;
                        }
                        else
                        {
                            parameterValues[i] = Convert.ChangeType(paramValue, paramInfo.ParameterType);
                        }
                    }
                    else if (paramInfo.HasDefaultValue)
                    {
                        parameterValues[i] = paramInfo.DefaultValue;
                    }
                    else
                    {
                        rpcMessage.ErrorMessage = "missing parameter " + paramInfo.Name;
                        return;
                    }
                }

                rpcMessage.Parameters = parameterValues;
            }
            else if (parameters is JArray parametersArray)
            {
                var parameterValues = new object[parameterDefs.Length];

                for (var i = 0; i < parameterValues.Length; i++)
                {
                    var paramInfo = parameterDefs[i];

                    if (i < parametersArray.Count)
                    {
                        parameterValues[i] = parametersArray[i].ToObject(paramInfo.ParameterType) ?? paramInfo.DefaultValue;
                    }
                    else if (paramInfo.HasDefaultValue)
                    {
                        parameterValues[i] = paramInfo.DefaultValue;
                    }
                    else
                    {
                        rpcMessage.ErrorMessage = "missing parameter at index " + i;
                        return;
                    }
                }

                rpcMessage.Parameters = parameterValues;
            }
        }

        private void ProcessJsonRpc(JsonReader reader, RpcRequestMessage rpcMessage)
        {
            rpcMessage.Version = reader.ReadAsString();
        }

        /// <summary>
        /// Determines whether this instance can convert the specified object type.
        /// </summary>
        /// <param name="objectType">Type of the object.</param>
        /// <returns>
        /// 	<c>true</c> if this instance can convert the specified object type; otherwise, <c>false</c>.
        /// </returns>
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(RpcRequestMessage);
        }
    }
}
