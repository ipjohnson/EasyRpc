using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using EasyRpc.AspNetCore.Errors;
using EasyRpc.AspNetCore.Serializers;
using Utf8 = Utf8Json;

namespace EasyRpc.AspNetCore.Utf8Json
{
    public class Utf8JsonContentSerializer : BaseSerializer
    {
        public Utf8JsonContentSerializer(IErrorHandler errorHandler) : base(errorHandler)
        {
        }

        public override IEnumerable<string> SupportedContentTypes => new[] { "application/json" };

        public override bool IsDefault => true;

        public override bool CanDeserialize(RequestExecutionContext context, string contentType)
        {
            return contentType.StartsWith("application/json", StringComparison.CurrentCultureIgnoreCase);
        }

        public override bool CanSerialize(RequestExecutionContext context, string accepts)
        {
            return accepts.Contains("application/json", StringComparison.CurrentCultureIgnoreCase);
        }

        public override Task SerializeToResponse(RequestExecutionContext context)
        {
            var serializedBytes = Utf8.JsonSerializer.NonGeneric.Serialize(context.Result);

            var response = context.HttpContext.Response;

            response.ContentType = "application/json";
            response.StatusCode = context.HttpStatusCode;
            response.ContentLength = serializedBytes.Length;

            return response.Body.WriteAsync(serializedBytes, 0, serializedBytes.Length, context.HttpContext.RequestAborted);
        }

        public override async ValueTask<T> DeserializeFromRequest<T>(RequestExecutionContext context)
        {
            return await Utf8.JsonSerializer.DeserializeAsync<T>(context.HttpContext.Request.Body);
        }
    }
}
