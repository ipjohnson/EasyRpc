using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using EasyRpc.AspNetCore.Configuration;
using EasyRpc.AspNetCore.Errors;
using Microsoft.Extensions.Primitives;

namespace EasyRpc.AspNetCore.Serializers
{
    public class JsonContentSerializer : BaseSerializer, IApiConfigurationCompleteAware
    {
        protected readonly IConfigurationManager ConfigurationManager;
        protected JsonSerializerOptions SerializerOptions;
        private const string _contentType = "application/json";

        public JsonContentSerializer(IErrorHandler errorHandler, IConfigurationManager configurationManager) : base(errorHandler)
        {
            ConfigurationManager = configurationManager;

            ConfigurationManager.CreationMethod(DefaultJsonSerializerOptions());
        }
        
        public override IEnumerable<string> SupportedContentTypes => new[] {_contentType};

        public override bool IsDefault => true;

        public override bool CanDeserialize(RequestExecutionContext context, string contentType)
        {
            return string.Compare(contentType, 0, "application/json", 0, _contentType.Length, StringComparison.InvariantCultureIgnoreCase) == 0;
        }

        public override bool CanSerialize(RequestExecutionContext context, string accepts)
        {
            return accepts.IndexOf("application/json", StringComparison.CurrentCultureIgnoreCase) >= 0;
        }

        public override async Task SerializeToResponse(RequestExecutionContext context)
        {
            SetSuccess(context, _contentType);

            var outputStream = GetOutputStream(context);

            await JsonSerializer.SerializeAsync(outputStream, context.Result, context.Result.GetType(), SerializerOptions, context.HttpContext.RequestAborted);

            if (outputStream != context.HttpContext.Response.Body)
            {
                await outputStream.FlushAsync(context.HttpContext.RequestAborted);

                outputStream.Dispose();
            }
        }

        public override ValueTask<T> DeserializeFromRequest<T>(RequestExecutionContext context)
        {
            if (!context.HttpContext.Request.Headers.TryGetValue("Content-Encoding", out var encoding))
            {
                return JsonSerializer.DeserializeAsync<T>(context.HttpContext.Request.Body, SerializerOptions,
                    context.HttpContext.RequestAborted);
            }

            return HandleContentEncoding<T>(context, encoding);
        }

        protected async ValueTask<T> HandleContentEncoding<T>(RequestExecutionContext context, string encoding)
        {
            if (encoding.Contains("br"))
            {
                using var brStream = new BrotliStream(context.HttpContext.Response.Body, CompressionMode.Decompress);

                return await JsonSerializer.DeserializeAsync<T>(brStream, null, context.HttpContext.RequestAborted);
            }
            
            if(encoding.Contains("gzip"))
            {
                using var gzipStream = new GZipStream(context.HttpContext.Response.Body, CompressionMode.Decompress);

                return await JsonSerializer.DeserializeAsync<T>(gzipStream, null, context.HttpContext.RequestAborted);
            }
            
            // fix this
            return await ErrorHandler.HandleDeserializeUnknownContentType<T>(context);
        }

        private Stream GetOutputStream(RequestExecutionContext context)
        {
            if (!context.CanCompress ||
                !context.HttpContext.Request.Headers.TryGetValue("Accept-Encoding", out var encoding))
            {
                return context.HttpContext.Response.Body;
            }

            return EncodeOutputStream(context, encoding);
        }

        private Stream EncodeOutputStream(RequestExecutionContext context, StringValues encoding)
        {
            switch (encoding)
            {
                case "gzip":
                case "GZIP":
                    context.HttpContext.Response.Headers["Content-Encoding"] = "gzip";
                    return new GZipStream(context.HttpContext.Response.Body, CompressionLevel.Fastest);

                case "br":
                case "BR":
                    context.HttpContext.Response.Headers["Content-Encoding"] = "br";
                    return new BrotliStream(context.HttpContext.Response.Body, CompressionLevel.Fastest);
            }

            return context.HttpContext.Response.Body;
        }

        private class JsonSerializerData
        {
            public Type DeserializeType { get; set; }
        }

        public void ApiConfigurationComplete(IServiceProvider serviceScope)
        {
            SerializerOptions = ConfigurationManager.GetConfiguration<JsonSerializerOptions>();
        }

        public static Func<JsonSerializerOptions> DefaultJsonSerializerOptions()
        {
            return () => new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        }
    }
}
