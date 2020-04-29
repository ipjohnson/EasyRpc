using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using EasyRpc.AspNetCore.EndPoints;
using EasyRpc.AspNetCore.ResponseHeader;
using EasyRpc.AspNetCore.Serializers;

namespace EasyRpc.AspNetCore.CodeGeneration
{
    public interface IResponseDelegateCreator
    {
        MethodEndPointDelegate CreateResponseDelegate(EndPointMethodConfiguration configuration);
    }

    /// <summary>
    /// Builds delegate that writes result to the response stream
    /// </summary>
    public class ResponseDelegateCreator : IResponseDelegateCreator
    {
        private readonly IRawContentWriter _rawContentWriter;

        private readonly MethodEndPointDelegate _defaultContentSerializer;
        
        public ResponseDelegateCreator(IContentSerializationService contentSerializer, IRawContentWriter rawContentWriter)
        {
            _rawContentWriter = rawContentWriter;

            _defaultContentSerializer = contentSerializer.SerializeToResponse;
        }

        public MethodEndPointDelegate CreateResponseDelegate(EndPointMethodConfiguration configuration)
        {
            if (string.IsNullOrEmpty(configuration.RawContentType))
            {
                if ((configuration.ResponseHeaders?.Count ?? 0) == 0)
                {
                    return _defaultContentSerializer;
                }

                return context => SerializeHeaderResponseWriter(context, configuration.ResponseHeaders);
            }

            var contentType = configuration.RawContentType;
            var encoding = configuration.RawContentEncoding;

            if ((configuration.ResponseHeaders?.Count ?? 0) == 0)
            {
                var writer = _rawContentWriter;

                return context => writer.WriteRawContent(context, contentType, encoding);
            }

            return context => RawHeaderResponseWriter(context, contentType, encoding, configuration.ResponseHeaders);
        }

        private Task SerializeHeaderResponseWriter(RequestExecutionContext context,
            IReadOnlyList<IResponseHeader> responseHeaders)
        {
            var headers = context.HttpContext.Response.Headers;

            foreach (var responseHeader in responseHeaders)
            {
                responseHeader.ApplyHeader(context, headers);
            }

            return _defaultContentSerializer(context);
        }
        
        private Task RawHeaderResponseWriter(RequestExecutionContext context, string contentType, string encodingType, IReadOnlyList<IResponseHeader> responseHeaders)
        {
            var headers = context.HttpContext.Response.Headers;

            foreach (var responseHeader in responseHeaders)
            {
                responseHeader.ApplyHeader(context, headers);
            }

            return _rawContentWriter.WriteRawContent(context, contentType, encodingType);
        }
    }
}
