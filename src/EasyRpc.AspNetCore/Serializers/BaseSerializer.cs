using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using EasyRpc.AspNetCore.Errors;

namespace EasyRpc.AspNetCore.Serializers
{
    public abstract class BaseSerializer : IContentSerializer
    {
        protected IErrorHandler ErrorHandler;
        protected int SerializerId;

        protected BaseSerializer(IErrorHandler errorHandler)
        {
            ErrorHandler = errorHandler;
        }

        /// <inheritdoc />
        public abstract IEnumerable<string> SupportedContentTypes { get; }

        /// <inheritdoc />
        public abstract bool IsDefault { get; }

        /// <inheritdoc />
        public abstract bool CanDeserialize(RequestExecutionContext context, string contentType);

        /// <inheritdoc />
        public abstract bool CanSerialize(RequestExecutionContext context, string accepts);

        /// <inheritdoc />
        public abstract Task SerializeToResponse(RequestExecutionContext context);

        /// <inheritdoc />
        public abstract ValueTask<T> DeserializeFromRequest<T>(RequestExecutionContext context);

        /// <summary>
        /// Set success code and content type in response
        /// </summary>
        /// <param name="context"></param>
        /// <param name="contentType"></param>
        protected void SetSuccess(RequestExecutionContext context, string contentType)
        {
            context.HttpContext.Response.ContentType = contentType;

            context.HttpContext.Response.StatusCode = context.HttpStatusCode;
        }

        public void AssignUniqueId(int id)
        {
            SerializerId = id;
        }
    }
}
