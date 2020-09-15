using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using EasyRpc.AspNetCore.Configuration;
using EasyRpc.AspNetCore.ContentEncoding;
using EasyRpc.AspNetCore.EndPoints;
using EasyRpc.AspNetCore.ResponseHeader;
using EasyRpc.AspNetCore.Serializers;

namespace EasyRpc.AspNetCore.CodeGeneration
{
    /// <summary>
    /// Response delegate creator
    /// </summary>
    public interface IResponseDelegateCreator
    {
        /// <summary>
        /// Create delegate that writes result to the response stream
        /// </summary>
        /// <param name="configuration"></param>
        /// <returns></returns>
        MethodEndPointDelegate CreateResponseDelegate(IEndPointMethodConfigurationReadOnly configuration);
    }

    /// <summary>
    /// Builds delegate that writes result to the response stream
    /// </summary>
    public class ResponseDelegateCreator : IResponseDelegateCreator, IApiConfigurationCompleteAware
    {
        /// <summary>
        /// No operation method
        /// </summary>
        protected static readonly MethodEndPointDelegate Noop = context => Task.CompletedTask;

        /// <summary>
        /// Content serializer
        /// </summary>
        protected readonly IContentSerializationService ContentSerializer;

        /// <summary>
        /// Raw Content Writer
        /// </summary>
        protected readonly IRawContentWriter RawContentWriter;

        /// <summary>
        /// Default content serializer method
        /// </summary>
        protected readonly MethodEndPointDelegate DefaultContentSerializer;

        /// <summary>
        /// Configuration manager
        /// </summary>
        protected readonly IConfigurationManager ConfigurationManager;

        /// <summary>
        /// Compression predicate provider
        /// </summary>
        protected readonly ICompressionPredicateProvider CompressionPredicateProvider;

        private ContentEncodingConfiguration _contentEncoding;

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="contentSerializer"></param>
        /// <param name="rawContentWriter"></param>
        /// <param name="configurationManager"></param>
        /// <param name="compressionPredicateProvider"></param>
        public ResponseDelegateCreator(IContentSerializationService contentSerializer,
            IRawContentWriter rawContentWriter,
            IConfigurationManager configurationManager, 
            ICompressionPredicateProvider compressionPredicateProvider)
        {
            ContentSerializer = contentSerializer;
            RawContentWriter = rawContentWriter;
            ConfigurationManager = configurationManager;
            CompressionPredicateProvider = compressionPredicateProvider;

            DefaultContentSerializer = contentSerializer.SerializeToResponse;
        }

        /// <inheritdoc />
        public virtual MethodEndPointDelegate CreateResponseDelegate(IEndPointMethodConfigurationReadOnly configuration)
        {
            if (string.IsNullOrEmpty(configuration.RawContentType))
            {
                if ((configuration.ResponseHeaders?.Count ?? 0) == 0)
                {
                    if (!configuration.SupportsCompression.GetValueOrDefault(false))
                    {
                        return DefaultContentSerializer;
                    }
                }

                Action<RequestExecutionContext> compressCheck = null;

                if (configuration.SupportsCompression.GetValueOrDefault(false))
                {
                    compressCheck = CompressionPredicateProvider.ProvideCompressionPredicate(configuration);
                }

                return context => SerializeHeaderResponseWriter(context, configuration.ResponseHeaders, compressCheck);
            }

            var contentType = configuration.RawContentType;
            var encoding = configuration.RawContentEncoding;

            if ((configuration.ResponseHeaders?.Count ?? 0) == 0)
            {
                var writer = RawContentWriter;

                return context => writer.WriteRawContent(context, contentType, encoding);
            }

            return context => RawHeaderResponseWriter(context, contentType, encoding, configuration.ResponseHeaders);
        }

        /// <summary>
        /// Serialize response as well as process headers
        /// </summary>
        /// <param name="context"></param>
        /// <param name="responseHeaders"></param>
        /// <param name="compressCheck"></param>
        /// <returns></returns>
        protected virtual Task SerializeHeaderResponseWriter(RequestExecutionContext context,
            IReadOnlyList<IResponseHeader> responseHeaders,
            Action<RequestExecutionContext> compressCheck)
        {
            var headers = context.HttpContext.Response.Headers;

            foreach (var responseHeader in responseHeaders)
            {
                responseHeader.ApplyHeader(context, headers);
            }

            compressCheck?.Invoke(context);

            return DefaultContentSerializer(context);
        }

        /// <summary>
        /// Write raw response with headers
        /// </summary>
        /// <param name="context"></param>
        /// <param name="contentType"></param>
        /// <param name="encodingType"></param>
        /// <param name="responseHeaders"></param>
        /// <returns></returns>
        protected virtual Task RawHeaderResponseWriter(RequestExecutionContext context, string contentType, string encodingType, IReadOnlyList<IResponseHeader> responseHeaders)
        {
            var headers = context.HttpContext.Response.Headers;

            foreach (var responseHeader in responseHeaders)
            {
                responseHeader.ApplyHeader(context, headers);
            }

            return RawContentWriter.WriteRawContent(context, contentType, encodingType);
        }

        public void ApiConfigurationComplete(IServiceProvider serviceScope)
        {
            _contentEncoding = ConfigurationManager.GetConfiguration<ContentEncodingConfiguration>();
        }
    }
}
