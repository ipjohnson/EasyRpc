using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using EasyRpc.AspNetCore.Messages;
using EasyRpc.AspNetCore.Middleware;
using Newtonsoft.Json;

namespace EasyRpc.AspNetCore.Converters
{
    public class RpcRequestMessageConverter : JsonConverter
    {
        protected class ConverterInfo
        {
            public ParamsDeserializer ParameterArrayDeserializer { get; set; }

            public ParamsDeserializer NamedParamsDeserializer { get; set; }
        }
        
        private EndPointConfiguration _endPointConfiguration;
        private IParameterArrayDeserializerBuilder _parameterArrayDeserializerBuilder;
        private INamedParameterDeserializerBuilder _namedParameterDeserializerBuilder;

        private ConcurrentDictionary<string, ConverterInfo> _exposedMethodInformations;

        public RpcRequestMessageConverter(EndPointConfiguration configuration, IParameterArrayDeserializerBuilder parameterArrayDeserializerBuilder, INamedParameterDeserializerBuilder namedParameterDeserializerBuilder)
        {
            _endPointConfiguration = configuration;
            _parameterArrayDeserializerBuilder = parameterArrayDeserializerBuilder;
            _namedParameterDeserializerBuilder = namedParameterDeserializerBuilder;
            _exposedMethodInformations = new ConcurrentDictionary<string, ConverterInfo>();
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
            var version = ReadVersion(reader);

            var method = ReadMethod(reader);

            var parameters = ReadParameters((RpcJsonReader)reader, method, serializer);

            var id = ReadId(reader);

            reader.Read();

            return new RpcRequestMessage { Version = version, Id = id, Method = method, Parameters = parameters };
        }

        private string ReadId(JsonReader reader)
        {
            if (reader.Read())
            {
                if (reader.TokenType == JsonToken.PropertyName && string.Compare("id", reader.Value as string,
                        StringComparison.CurrentCultureIgnoreCase) == 0)
                {
                    return reader.ReadAsString();
                }
            }

            return "";
        }

        private object[] ReadParameters(RpcJsonReader reader, string method, JsonSerializer serializer)
        {
            if (reader.Read())
            {
                if (reader.TokenType == JsonToken.PropertyName && string.Compare("params", reader.Value as string,
                        StringComparison.CurrentCultureIgnoreCase) == 0)
                {
                    reader.Read();

                    if (reader.TokenType == JsonToken.StartArray)
                    {
                        reader.Read();

                        var deserializer = GetArrayDeserializer(reader.UrlPath, method);
                        
                        return deserializer?.Invoke(reader, serializer);
                    }

                    if (reader.TokenType == JsonToken.StartObject)
                    {
                        reader.Read();

                        var deserializer = GetNamedParameterDeserializer(reader.UrlPath, method);

                        return deserializer?.Invoke(reader, serializer);
                    }

                    if (reader.TokenType == JsonToken.Null)
                    {
                        return new object[0];
                    }
                }
            }

            throw new Exception("Could not read parameters");
        }

        private ParamsDeserializer GetNamedParameterDeserializer(string urlPath, string method)
        {
            var key = string.Concat(urlPath, '*', method);

            if (_exposedMethodInformations.TryGetValue(key, out var converter))
            {
                if (converter.NamedParamsDeserializer != null)
                {
                    return converter.NamedParamsDeserializer;
                }
            }

            if (_endPointConfiguration.Methods.TryGetValue(key, out var exposedMethod))
            {
                var paramsDeserializer = _namedParameterDeserializerBuilder.BuildDeserializer(exposedMethod);

                if (converter == null)
                {
                    _exposedMethodInformations.TryAdd(key,
                        new ConverterInfo { NamedParamsDeserializer = paramsDeserializer });
                }
                else
                {
                    converter.NamedParamsDeserializer = paramsDeserializer;
                }

                return paramsDeserializer;
            }

            return null;
        }

        private ParamsDeserializer GetArrayDeserializer(string url, string method)
        {
            var key = string.Concat(url, '*', method);

            if (_exposedMethodInformations.TryGetValue(key, out var converter))
            {
                if (converter.ParameterArrayDeserializer != null)
                {
                    return converter.ParameterArrayDeserializer;
                }
            }

            if (_endPointConfiguration.Methods.TryGetValue(key, out var exposedMethod))
            {
                var paramsDeserializer = _parameterArrayDeserializerBuilder.BuildDeserializer(exposedMethod);

                if (converter == null)
                {
                    _exposedMethodInformations.TryAdd(key,
                        new ConverterInfo {ParameterArrayDeserializer = paramsDeserializer});
                }
                else
                {
                    converter.ParameterArrayDeserializer = paramsDeserializer;
                }

                return paramsDeserializer;
            }

            return null;
        }

        private string ReadMethod(JsonReader reader)
        {
            if (reader.Read())
            {
                if (reader.TokenType == JsonToken.PropertyName && string.Compare("method", reader.Value as string,
                        StringComparison.CurrentCultureIgnoreCase) == 0)
                {
                    return reader.ReadAsString();
                }
            }

            throw new Exception("Could not find method");
        }

        private string ReadVersion(JsonReader reader)
        {
            if (reader.Read())
            {
                if (reader.TokenType == JsonToken.PropertyName && string.Compare("jsonrpc", reader.Value as string,
                        StringComparison.CurrentCultureIgnoreCase) == 0)
                {
                    return reader.ReadAsString();
                }
            }

            return "";
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
