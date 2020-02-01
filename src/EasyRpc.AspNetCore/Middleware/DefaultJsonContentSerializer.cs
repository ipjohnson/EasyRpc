using System;
using System.IO;
using System.Threading.Tasks;
using EasyRpc.AspNetCore.Content;
using EasyRpc.AspNetCore.Converters;
using EasyRpc.AspNetCore.Messages;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace EasyRpc.AspNetCore.Middleware
{
    public class DefaultJsonContentSerializer : IContentSerializer
    {
        private JsonSerializer _serializer;
        private readonly IParameterArrayDeserializerBuilder _parameterArrayDeserializer;
        private readonly INamedParameterDeserializerBuilder _namedParameterDeserializer;

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="parameterArrayDeserializer"></param>
        /// <param name="namedParameterDeserializer"></param>
        /// <param name="serializer"></param>
        public DefaultJsonContentSerializer(IParameterArrayDeserializerBuilder parameterArrayDeserializer, 
            INamedParameterDeserializerBuilder namedParameterDeserializer, JsonSerializer serializer)
        {
            _parameterArrayDeserializer = parameterArrayDeserializer;
            _namedParameterDeserializer = namedParameterDeserializer;
            _serializer = serializer;
        }

        /// <summary>
        /// ContentType to encode/decode
        /// </summary>
        public virtual string ContentType => "application/json";

        /// <summary>
        /// Serializer id assigned by framework
        /// </summary>
        public int SerializerId { get; set; }

        /// <summary>
        /// Configure content serializer
        /// </summary>
        /// <param name="configuration"></param>
        public void Configure(IExposeMethodInformationCacheManager configuration)
        {
            _serializer.Converters.Add(new UnorderedRpcRequestMessageConverter(_parameterArrayDeserializer, _namedParameterDeserializer, configuration,SerializerId));
            _serializer.Converters.Add(new RpcRequestPackageConverter());
        }

        /// <summary>
        /// Seriaize the response to the outputStream
        /// </summary>
        /// <param name="outputStream"></param>
        /// <param name="response"></param>
        /// <param name="context"></param>
        public async Task SerializeResponse(Stream outputStream, object response, HttpContext context)
        {
            using (var memoryStream = new MemoryStream())
            {
                using (var textStream = new StreamWriter(memoryStream))
                {
                    using (var jsonWriter = new JsonTextWriter(textStream))
                    {
                        _serializer.Serialize(jsonWriter, response);

                        await jsonWriter.FlushAsync();
                        await textStream.FlushAsync();

                        memoryStream.Position = 0;

                        await memoryStream.CopyToAsync(outputStream);
                    }
                }

            }
        }

        /// <summary>
        /// Deserialize the input stream to a request package
        /// </summary>
        /// <param name="inputStream">input stream</param>
        /// <param name="path"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task<RpcRequestPackage> DeserializeRequestPackage(Stream inputStream, string path, HttpContext context)
        {
            using (var memoryStream = new MemoryStream())
            {
                await inputStream.CopyToAsync(memoryStream);

                memoryStream.Position = 0;

                using (var textStream = new StreamReader(memoryStream))
                {
                    using (var rpcJsonReader = new RpcJsonReader(textStream, path, context))
                    {
                        return _serializer.Deserialize<RpcRequestPackage>(rpcJsonReader);
                    }
                }
            }
        }

        /// <summary>
        /// Can serialize 
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public bool CanSerialize(HttpContext context)
        {
            return context.Request.ContentType.StartsWith("application/json",
                StringComparison.CurrentCultureIgnoreCase);
        }
    }
}
