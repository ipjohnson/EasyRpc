using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using EasyRpc.AspNetCore.Errors;

namespace EasyRpc.AspNetCore.Serializers
{
    /// <summary>
    /// Abstract base class for content serializer
    /// </summary>
    public abstract class BaseSerializer : IContentSerializer
    {
        /// <summary>
        /// Error handler for serializer
        /// </summary>
        protected IErrorHandler ErrorHandler;

        /// <summary>
        /// Serializer id
        /// </summary>
        protected int SerializerId;

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="errorHandler"></param>
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

        /// <summary>
        /// Assign serialization id
        /// </summary>
        /// <param name="id"></param>
        public void AssignUniqueId(int id)
        {
            SerializerId = id;
        }
    }
}
