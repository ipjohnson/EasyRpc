using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using EasyRpc.AspNetCore.Messages;
using EasyRpc.AspNetCore.Middleware;
using Newtonsoft.Json;

namespace EasyRpc.AspNetCore.Converters
{
    /// <summary>
    /// Json converter for RpcRequestMessage, it will only process messages that are in the order of jsonrpc, method, params, id
    /// </summary>
    public class StrictRpcRequestMessageConverter : JsonConverter
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

        private ConcurrentDictionary<string, ConverterInfo> _exposedMethodInformations;

        public StrictRpcRequestMessageConverter(
            IParameterArrayDeserializerBuilder parameterArrayDeserializerBuilder,
            INamedParameterDeserializerBuilder namedParameterDeserializerBuilder,
            IExposeMethodInformationCacheManager cacheManager,
            int serializerId)
        {
            _parameterArrayDeserializerBuilder = parameterArrayDeserializerBuilder;
            _namedParameterDeserializerBuilder = namedParameterDeserializerBuilder;
            _cacheManager = cacheManager;
            _serializerId = serializerId;
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

            var path = ((RpcJsonReader)reader).UrlPath;

            var converter = GetConverter(path, method);

            if (converter == null)
            {
                return new RpcRequestMessage { Version = version, ErrorMessage = $"Method not found {method} at {path}" };
            }

            var parameters = GetParameters((RpcJsonReader)reader, converter, serializer);

            var id = ReadId(reader);

            reader.Read();

            return new RpcRequestMessage
            {
                Version = version,
                Id = id,
                Method = method,
                Parameters = parameters,
                MethodInformation = converter.ExposedMethod
            };
        }

        private object[] GetParameters(RpcJsonReader reader, ConverterInfo converter, JsonSerializer serializer)
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
            }

            throw new Exception("Could not read parameters");
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
