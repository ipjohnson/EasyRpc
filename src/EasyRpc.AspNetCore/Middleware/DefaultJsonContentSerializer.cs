using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using EasyRpc.AspNetCore.Content;
using EasyRpc.AspNetCore.Converters;
using EasyRpc.AspNetCore.Messages;
using Newtonsoft.Json;

namespace EasyRpc.AspNetCore.Middleware
{
    public class DefaultJsonContentSerializer : IContentSerializer
    {
        private JsonSerializer _serializer;
        private IParameterArrayDeserializerBuilder _parameterArrayDeserializer;
        private INamedParameterDeserializerBuilder _namedParameterDeserializer;

        public DefaultJsonContentSerializer(IParameterArrayDeserializerBuilder parameterArrayDeserializer, INamedParameterDeserializerBuilder namedParameterDeserializer)
        {
            _parameterArrayDeserializer = parameterArrayDeserializer;
            _namedParameterDeserializer = namedParameterDeserializer;
        }

        /// <summary>
        /// ContentType to encode/decode
        /// </summary>
        public virtual string ContentType => "application/json";

        /// <summary>
        /// Configure content serializer
        /// </summary>
        /// <param name="configuration"></param>
        public void Configure(EndPointConfiguration configuration)
        {
            _serializer = new JsonSerializer();
            _serializer.Converters.Add(new RpcRequestMessageConverter(configuration, _parameterArrayDeserializer, _namedParameterDeserializer));
            _serializer.Converters.Add(new RpcRequestPackageConverter());
        }

        /// <summary>
        /// Seriaize the response to the outputStream
        /// </summary>
        /// <param name="outputStream"></param>
        /// <param name="response"></param>
        public void SerializeResponse(Stream outputStream, object response)
        {
            using (var textStream = new StreamWriter(outputStream))
            {
                using (var jsonWriter = new JsonTextWriter(textStream))
                {
                    _serializer.Serialize(jsonWriter, response);
                }
            }
        }

        /// <summary>
        /// Deserialize the input stream to a request package
        /// </summary>
        /// <param name="inputStream">input stream</param>
        /// <param name="path"></param>
        /// <returns></returns>
        public RpcRequestPackage DeserializeRequestPackage(Stream inputStream, string path)
        {
            using (var textStream = new StreamReader(inputStream))
            {
                using (var rpcJsonReader = new RpcJsonReader(textStream,path))
                {
                    return _serializer.Deserialize<RpcRequestPackage>(rpcJsonReader);
                }
            }
        }
    }
}
