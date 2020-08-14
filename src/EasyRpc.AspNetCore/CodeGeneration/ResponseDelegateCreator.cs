using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
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
    public class ResponseDelegateCreator : IResponseDelegateCreator
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
        /// Default constructor
        /// </summary>
        /// <param name="contentSerializer"></param>
        /// <param name="rawContentWriter"></param>
        public ResponseDelegateCreator(IContentSerializationService contentSerializer, IRawContentWriter rawContentWriter)
        {
            ContentSerializer = contentSerializer;
            RawContentWriter = rawContentWriter;

            DefaultContentSerializer = contentSerializer.SerializeToResponse;
        }

        /// <inheritdoc />
        public virtual MethodEndPointDelegate CreateResponseDelegate(IEndPointMethodConfigurationReadOnly configuration)
        {
            if (string.IsNullOrEmpty(configuration.RawContentType))
            {
                if ((configuration.ResponseHeaders?.Count ?? 0) == 0)
                {
                    return DefaultContentSerializer;
                }

                return context => SerializeHeaderResponseWriter(context, configuration.ResponseHeaders);
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
        /// <returns></returns>
        protected virtual Task SerializeHeaderResponseWriter(RequestExecutionContext context,
            IReadOnlyList<IResponseHeader> responseHeaders)
        {
            var headers = context.HttpContext.Response.Headers;

            foreach (var responseHeader in responseHeaders)
            {
                responseHeader.ApplyHeader(context, headers);
            }

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
    }
}
