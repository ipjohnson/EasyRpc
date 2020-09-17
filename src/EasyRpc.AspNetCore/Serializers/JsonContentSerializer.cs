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
    /// <summary>
    /// Default json content serializer based on System.Text.Json
    /// </summary>
    public class JsonContentSerializer : BaseSerializer, IApiConfigurationCompleteAware
    {
        private readonly IConfigurationManager _configurationManager;
        private JsonSerializerOptions _serializerOptions;
        private const string _contentType = "application/json";

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="errorHandler"></param>
        /// <param name="configurationManager"></param>
        public JsonContentSerializer(IErrorHandler errorHandler, IConfigurationManager configurationManager) : base(
            errorHandler)
        {
            _configurationManager = configurationManager;

            _configurationManager.CreationMethod(DefaultJsonSerializerOptions());
        }

        /// <inheritdoc />
        public override IEnumerable<string> SupportedContentTypes => new[] {_contentType};

        /// <inheritdoc />
        public override bool IsDefault { get; set; } = true;

        /// <inheritdoc />
        public override bool CanDeserialize(RequestExecutionContext context, string contentType)
        {
            return string.Compare(contentType, 0, _contentType, 0, _contentType.Length, StringComparison.InvariantCultureIgnoreCase) == 0;
        }

        /// <inheritdoc />
        public override bool CanSerialize(RequestExecutionContext context, string accepts)
        {
            return accepts != null &&
                accepts.IndexOf(_contentType, StringComparison.CurrentCultureIgnoreCase) >= 0;
        }

        /// <inheritdoc />
        public override async Task SerializeToResponse(RequestExecutionContext context)
        {
            SetStatusAndContent(context, _contentType);

            var outputStream = GetOutputStream(context);

            await JsonSerializer.SerializeAsync(outputStream, context.Result, context.Result.GetType(), _serializerOptions, context.HttpContext.RequestAborted);

            if (outputStream != context.HttpContext.Response.Body)
            {
                await outputStream.FlushAsync(context.HttpContext.RequestAborted);

                outputStream.Dispose();
            }
        }

        /// <inheritdoc />
        public override ValueTask<T> DeserializeFromRequest<T>(RequestExecutionContext context)
        {
            if (!context.HttpContext.Request.Headers.TryGetValue("Content-Encoding", out var encoding))
            {
                return JsonSerializer.DeserializeAsync<T>(context.HttpContext.Request.Body, _serializerOptions,
                    context.HttpContext.RequestAborted);
            }

            return HandleContentEncoding<T>(context, encoding);
        }

        /// <inheritdoc />
        public void ApiConfigurationComplete(IServiceProvider serviceScope)
        {
            _serializerOptions = _configurationManager.GetConfiguration<JsonSerializerOptions>();
        }

        /// <summary>
        /// Method that provides default json settings
        /// </summary>
        /// <returns></returns>
        public static Func<JsonSerializerOptions> DefaultJsonSerializerOptions()
        {
            return () => new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        }

        private async ValueTask<T> HandleContentEncoding<T>(RequestExecutionContext context, string encoding)
        {
            if (encoding.Contains("br"))
            {
                await using var brStream = new BrotliStream(context.HttpContext.Response.Body, CompressionMode.Decompress);

                return await JsonSerializer.DeserializeAsync<T>(brStream, null, context.HttpContext.RequestAborted);
            }
            
            if(encoding.Contains("gzip"))
            {
                await using var gzipStream = new GZipStream(context.HttpContext.Response.Body, CompressionMode.Decompress);

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
    }
}
