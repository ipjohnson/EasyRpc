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
        public virtual string ContentType => "application/json";

        public virtual void SerializeToRequest(object body, HttpRequestMessage request, bool compressBody)
        {
            var bytes = JsonSerializer.SerializeToUtf8Bytes(body, body.GetType());

            if (compressBody && bytes.Length > 1200)
            {
                using var memoryStream = new MemoryStream();
                using var compressedStream = new BrotliStream(memoryStream, (CompressionLevel)6);

                compressedStream.Write(bytes);

                request.Content = new ByteArrayContent(memoryStream.ToArray());

                request.Headers.Add("Content-Encoding","br");
            }
            else
            {
                request.Content = new ByteArrayContent(bytes);
            }

            request.Content.Headers.ContentType = new MediaTypeHeaderValue(ContentType);
        }

        public virtual async Task<T> DeserializeFromResponse<T>(HttpResponseMessage responseMessage)
        {
            await using var readStream = await responseMessage.Content.ReadAsStreamAsync();
            
            var responseStream = readStream;
            
            if (responseMessage.Headers.TryGetValues("Content-Encoding", out var contentEncoding))
            {
                var contentEncodingList = contentEncoding.ToList();
                if (contentEncodingList.Contains("gzip"))
                {
                    await using var gzipStream = new GZipStream(readStream, CompressionMode.Decompress);

                    responseStream = gzipStream;
                }
                else if(contentEncodingList.Contains("br"))
                {
                    await using var brStream = new BrotliStream(readStream, CompressionMode.Decompress);

                    responseStream = brStream;
                }
                else
                {
                    throw new Exception($"Unknown content encoding {contentEncodingList}");
                }
            }

            return await JsonSerializer.DeserializeAsync<T>(responseStream);
        }
    }

#endif

}
