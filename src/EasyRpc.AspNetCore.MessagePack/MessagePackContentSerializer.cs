using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EasyRpc.AspNetCore.Serializers;
using MsgPack = MessagePack;

namespace EasyRpc.AspNetCore.MessagePack
{
    public class MessagePackContentSerializer : IContentSerializer
    {
        private readonly MsgPack.MessagePackSerializerOptions _options;

        /// <summary>
        /// Content type for content type
        /// </summary>
        protected string ContentType = "application/msgpack";

        public MessagePackContentSerializer()
        {
            _options = MsgPack.MessagePackSerializerOptions.Standard;
        }

        /// <inheritdoc />
        public IEnumerable<string> SupportedContentTypes => new[] { ContentType };

        /// <inheritdoc />
        public bool IsDefault => false;

        /// <inheritdoc />
        public bool CanDeserialize(RequestExecutionContext context, string contentType)
        {
            return contentType.StartsWith(ContentType, StringComparison.CurrentCultureIgnoreCase);
        }

        /// <inheritdoc />
        public bool CanSerialize(RequestExecutionContext context, string accepts)
        {
            return accepts.Contains(ContentType, StringComparison.CurrentCultureIgnoreCase);
        }

        /// <inheritdoc />
        public Task SerializeToResponse(RequestExecutionContext context)
        {
            var response = context.HttpContext.Response;
            
            response.ContentType = ContentType;
            response.StatusCode = context.HttpStatusCode;

            return MsgPack.MessagePackSerializer.SerializeAsync(response.Body, context.Result, _options, context.HttpContext.RequestAborted);
        }

        /// <inheritdoc />
        public ValueTask<T> DeserializeFromRequest<T>(RequestExecutionContext context)
        {
            return MsgPack.MessagePackSerializer.DeserializeAsync<T>(context.HttpContext.Request.Body, _options, context.HttpContext.RequestAborted);
        }
    }
}
