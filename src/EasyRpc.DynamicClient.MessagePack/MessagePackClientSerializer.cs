using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using EasyRpc.DynamicClient.Serializers;
using MsgPack = MessagePack;


namespace EasyRpc.DynamicClient.MessagePack
{
    public class MessagePackClientSerializer : IClientSerializer
    {
        /// <inheritdoc />
        public string ContentType => "application/msgpack";

        /// <inheritdoc />
        public Task SerializeToRequest(object body, HttpRequestMessage request, bool compressBody)
        {
            var bytes = MsgPack.MessagePackSerializer.Serialize(body);

            var byteArrayContent = new ByteArrayContent(bytes);
            
            byteArrayContent.Headers.ContentType = new MediaTypeHeaderValue(ContentType);

            request.Content = byteArrayContent;

            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public async Task<T> DeserializeFromResponse<T>(HttpResponseMessage responseMessage)
        {
            return await MsgPack.MessagePackSerializer.DeserializeAsync<T>(await responseMessage.Content.ReadAsStreamAsync());
        }

        /// <inheritdoc />
        public Task<byte[]> Serialize(object value)
        {
            return Task.FromResult(MsgPack.MessagePackSerializer.Serialize(value));
        }

        /// <inheritdoc />
        public Task<T> Deserialize<T>(byte[] bytes)
        {
            using (var stream = new MemoryStream(bytes))
            {
                return Task.FromResult(MsgPack.MessagePackSerializer.Deserialize<T>(stream));
            }
        }
    }
}
