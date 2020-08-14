using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace EasyRpc.DynamicClient.Serializers
{

#if NETCOREAPP3_1
    using System.Text.Json;

    public class JsonClientSerializer : IClientSerializer
    {
        public static readonly JsonClientSerializer DefaultSerializer = new JsonClientSerializer();

        public JsonSerializerOptions DefaultJsonSerializerOptions { get; set; } = new JsonSerializerOptions();

        public virtual string ContentType { get; set; } = "application/json";

        public virtual Task SerializeToRequest(object body, HttpRequestMessage request, bool compressBody)
        {
            var bytes = JsonSerializer.SerializeToUtf8Bytes(body, body.GetType(), DefaultJsonSerializerOptions);

            if (compressBody && bytes.Length > 1200)
            {
                using var memoryStream = new MemoryStream();
                using var compressedStream = new BrotliStream(memoryStream, (CompressionLevel)6);

                compressedStream.Write(bytes);

                request.Content = new ByteArrayContent(memoryStream.ToArray());

                request.Headers.Add("Content-Encoding", "br");
            }
            else
            {
                request.Content = new ByteArrayContent(bytes);
            }

            request.Content.Headers.ContentType = new MediaTypeHeaderValue(ContentType);

            request.Headers.Add("Accept-Encoding", "br");

            return Task.CompletedTask;
        }

        public virtual async Task<T> DeserializeFromResponse<T>(HttpResponseMessage responseMessage)
        {
            await using var readStream = await responseMessage.Content.ReadAsStreamAsync();

            var responseStream = readStream;

            if (responseMessage.Headers.TryGetValues("Content-Encoding", out var contentEncoding))
            {
                var contentEncodingList = contentEncoding.ToList();

                if (contentEncodingList.Contains("br"))
                {
                    await using var brStream = new BrotliStream(readStream, CompressionMode.Decompress);

                    responseStream = brStream;
                }
                else if (contentEncodingList.Contains("gzip"))
                {
                    await using var gzipStream = new GZipStream(readStream, CompressionMode.Decompress);

                    responseStream = gzipStream;
                }
                else
                {
                    throw new Exception($"Unknown content encoding {contentEncodingList}");
                }
            }

            var stringResult = await responseMessage.Content.ReadAsStringAsync();

            return await JsonSerializer.DeserializeAsync<T>(responseStream, DefaultJsonSerializerOptions);
        }

        /// <inheritdoc />
        public async Task<byte[]> Serialize(object value)
        {
            return JsonSerializer.SerializeToUtf8Bytes(value, DefaultJsonSerializerOptions);
        }

        /// <inheritdoc />
        public async Task<T> Deserialize<T>(byte[] bytes)
        {
            return JsonSerializer.Deserialize<T>(bytes, DefaultJsonSerializerOptions);
        }
    }

#endif

}
