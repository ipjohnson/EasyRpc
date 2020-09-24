using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using EasyRpc.DynamicClient.Serializers;
using Newtonsoft.Json;

namespace EasyRpc.DynamicClient.Newtonsoft
{
    public class NewtonsoftClientSerializer : IClientSerializer
    {
        public static readonly NewtonsoftClientSerializer DefaultSerializer = new NewtonsoftClientSerializer();

        public NewtonsoftClientSerializer()
        {
            JsonSerializer = new JsonSerializer();
        }

        public NewtonsoftClientSerializer(JsonSerializer serializer)
        {
            JsonSerializer = serializer;
        }

        public JsonSerializer JsonSerializer { get; set; }

        /// <inheritdoc />
        public string ContentType { get; set; } = "application/json";

        /// <inheritdoc />
        public Task SerializeToRequest(object body, HttpRequestMessage request, bool compressBody)
        {
            using (var memoryStream = new MemoryStream())
            {
                using (var textStream = new StreamWriter(memoryStream))
                {
                    JsonSerializer.Serialize(textStream, body);
                }

                var byteArray = memoryStream.ToArray();

                request.Content = new ByteArrayContent(byteArray);
                request.Content.Headers.ContentType = new MediaTypeHeaderValue(ContentType);
                request.Content.Headers.ContentLength = byteArray.Length;
            }

            return default;
        }

        /// <inheritdoc />
        public async Task<T> DeserializeFromResponse<T>(HttpResponseMessage responseMessage)
        {
            using (var readStream = await responseMessage.Content.ReadAsStreamAsync())
            {
                var responseStream = readStream;

                try
                {
                    if (responseMessage.Headers.TryGetValues("Content-Encoding", out var contentEncoding))
                    {
                        var contentEncodingList = contentEncoding.ToList();

                        if (contentEncodingList.Contains("gzip"))
                        {
                            var gzipStream = new GZipStream(readStream, CompressionMode.Decompress);

                            responseStream = gzipStream;
                        }
                        else
                        {
                            throw new Exception($"Unknown content encoding {contentEncodingList}");
                        }
                    }

                    using (var textReader = new StreamReader(responseStream))
                    {
                        using (var jsonStream = new JsonTextReader(textReader))
                        {
                            return JsonSerializer.Deserialize<T>(jsonStream);
                        }
                    }
                }
                finally
                {
                    if (responseStream != readStream)
                    {
                        responseStream.Dispose();
                    }
                }
            }
        }

        /// <inheritdoc />
        public Task<byte[]> Serialize(object value)
        {
            using (var memoryStream = new MemoryStream())
            {
                using (var textStream = new StreamWriter(memoryStream))
                {
                    JsonSerializer.Serialize(textStream, value);
                }

                return Task.FromResult(memoryStream.ToArray());
            }
        }

        /// <inheritdoc />
        public Task<T> Deserialize<T>(byte[] bytes)
        {
            using (var memoryStream = new MemoryStream(bytes))
            {
                using (var textStream = new StreamReader(memoryStream))
                {
                    using (var jsonRead = new JsonTextReader(textStream))
                    {
                        return Task.FromResult(JsonSerializer.Deserialize<T>(jsonRead));
                    }
                }
            }
        }
    }
}
