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

        public abstract IEnumerable<string> SupportedContentTypes { get; }

        public abstract bool IsDefault { get; }

        public abstract bool CanDeserialize(RequestExecutionContext context, string contentType);

        public abstract bool CanSerialize(RequestExecutionContext context, string accepts);

        public abstract Task SerializeToResponse(RequestExecutionContext context);

        public abstract ValueTask<T> DeserializeFromRequest<T>(RequestExecutionContext context);

        
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
