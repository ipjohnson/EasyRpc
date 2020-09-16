using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text.Json;
using System.Threading.Tasks;
using EasyRpc.AspNetCore.Configuration;
using EasyRpc.AspNetCore.Errors;
using EasyRpc.AspNetCore.Serializers;
using Microsoft.Extensions.Primitives;
using Json = Newtonsoft.Json;

namespace EasyRpc.AspNetCore.Newtonsoft
{
    public class NewtonsoftContentSerializer : BaseSerializer
    {
        protected readonly IConfigurationManager ConfigurationManager;
        protected readonly Json.JsonSerializer JsonSerializer;
        private const string _contentType = "application/json";

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="errorHandler"></param>
        /// <param name="configurationManager"></param>
        public NewtonsoftContentSerializer(IErrorHandler errorHandler, IConfigurationManager configurationManager) : base(errorHandler)
        {
            ConfigurationManager = configurationManager;
            

            ConfigurationManager.CreationMethod(CreateJsonSerializer);
        }

        /// <inheritdoc />
        public override IEnumerable<string> SupportedContentTypes => new[] { _contentType };

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
            return accepts.IndexOf(_contentType, StringComparison.CurrentCultureIgnoreCase) >= 0;
        }

        /// <inheritdoc />
        public override async Task SerializeToResponse(RequestExecutionContext context)
        {
            SetStatusAndContent(context, _contentType);

            var outputStream = GetOutputStream(context);

            await using var memoryStream = new MemoryStream();

            await using var textStream = new StreamWriter(memoryStream);

            JsonSerializer.Serialize(textStream, context.Result);

            await memoryStream.CopyToAsync(outputStream, context.HttpContext.RequestAborted);

            if (outputStream != context.HttpContext.Response.Body)
            {
                await outputStream.FlushAsync(context.HttpContext.RequestAborted);

                outputStream.Dispose();
            }
        }

        /// <inheritdoc />
        public override async ValueTask<T> DeserializeFromRequest<T>(RequestExecutionContext context)
        {
            if (!context.HttpContext.Request.Headers.TryGetValue("Content-Encoding", out var encoding))
            {
                return await DeserializeStream<T>(context, context.HttpContext.Request.Body);
            }

            return await HandleContentEncoding<T>(context, encoding);
        }

        private async Task<T> DeserializeStream<T>(RequestExecutionContext context, Stream stream)
        {
            await using var memoryStream = new MemoryStream();

            await stream.CopyToAsync(memoryStream, context.HttpContext.RequestAborted);

            memoryStream.Position = 0;

            using var streamReader = new StreamReader(memoryStream);

            using var jsonReader = new Json.JsonTextReader(streamReader);

            return JsonSerializer.Deserialize<T>(jsonReader);
        }

        protected async ValueTask<T> HandleContentEncoding<T>(RequestExecutionContext context, string encoding)
        {
            if (encoding.Contains("br"))
            {
                await using var brStream = new BrotliStream(context.HttpContext.Response.Body, CompressionMode.Decompress);

                return await DeserializeStream<T>(context, brStream);
            }

            if (encoding.Contains("gzip"))
            {
                await using var gzipStream = new GZipStream(context.HttpContext.Response.Body, CompressionMode.Decompress);

                return await DeserializeStream<T>(context, gzipStream);
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

        private Json.JsonSerializer CreateJsonSerializer()
        {
            return new Json.JsonSerializer();
        }

    }
}
