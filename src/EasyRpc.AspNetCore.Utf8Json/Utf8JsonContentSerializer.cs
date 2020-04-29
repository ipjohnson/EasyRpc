using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using EasyRpc.AspNetCore.Errors;
using EasyRpc.AspNetCore.Serializers;
using Utf8 = Utf8Json;

namespace EasyRpc.AspNetCore.Utf8Json
{
    /// <summary>
    /// Utf8Json serializer
    /// </summary>
    public class Utf8JsonContentSerializer : BaseSerializer
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="errorHandler"></param>
        public Utf8JsonContentSerializer(IErrorHandler errorHandler) : base(errorHandler)
        {
        }

        /// <summary>
        /// Supports application/json
        /// </summary>
        public override IEnumerable<string> SupportedContentTypes => new[] { "application/json" };

        /// <summary>
        /// This is a default serializer
        /// </summary>
        public override bool IsDefault => true;

        /// <inheritdoc />
        public override bool CanDeserialize(RequestExecutionContext context, string contentType)
        {
            return contentType.StartsWith("application/json", StringComparison.CurrentCultureIgnoreCase);
        }

        /// <inheritdoc />
        public override bool CanSerialize(RequestExecutionContext context, string accepts)
        {
            return accepts.Contains("application/json", StringComparison.CurrentCultureIgnoreCase);
        }

        /// <inheritdoc />
        public override Task SerializeToResponse(RequestExecutionContext context)
        {
            var serializedBytes = Utf8.JsonSerializer.NonGeneric.Serialize(context.Result);

            var response = context.HttpContext.Response;

            response.ContentType = "application/json";
            response.StatusCode = context.HttpStatusCode;
            response.ContentLength = serializedBytes.Length;

            return response.Body.WriteAsync(serializedBytes, 0, serializedBytes.Length, context.HttpContext.RequestAborted);
        }

        /// <inheritdoc />
        public override async ValueTask<T> DeserializeFromRequest<T>(RequestExecutionContext context)
        {
            return await Utf8.JsonSerializer.DeserializeAsync<T>(context.HttpContext.Request.Body);
        }
    }
}
