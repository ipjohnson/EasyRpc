using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipelines;
using System.Text;
using System.Threading.Tasks;
using EasyRpc.AspNetCore.EndPoints;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace EasyRpc.AspNetCore.Serializers
{
    /// <summary>
    /// Interface for service that writes raw content to response stream
    /// </summary>
    public interface IRawContentWriter
    {
        /// <summary>
        /// Write result from requestContext to response stream as raw bytes without serialization
        /// </summary>
        /// <param name="requestContext"></param>
        /// <param name="contentType"></param>
        /// <param name="contentEncoding"></param>
        /// <returns></returns>
        Task WriteRawContent(RequestExecutionContext requestContext, string contentType, string contentEncoding);
    }

    /// <summary>
    /// Raw content writing service
    /// </summary>
    public class RawContentWriter : IRawContentWriter
    {
        private readonly ICustomActionResultExecutor _actionResultExecutor;

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="actionResultExecutor"></param>
        public RawContentWriter(ICustomActionResultExecutor actionResultExecutor)
        {
            _actionResultExecutor = actionResultExecutor;
        }

        /// <inheritdoc />
        public Task WriteRawContent(RequestExecutionContext requestContext, string contentType, string contentEncoding)
        {
            if (requestContext.Result is IActionResult actionResult)
            {
                return _actionResultExecutor.Execute(requestContext, actionResult);
            }

            var response = requestContext.HttpContext.Response;

            response.StatusCode = requestContext.HttpStatusCode;

            response.ContentType = contentType;

            if (contentEncoding != null)
            {
                response.Headers.TryAdd("Content-Encoding", contentEncoding);
            }

            if (requestContext.Result is byte[] bytes)
            {
                return ReturnByteArray(response, bytes);
            }

            if (requestContext.Result is string resultString)
            {
                return ReturnByteArray(response, Encoding.UTF8.GetBytes(resultString));
            }

            if (requestContext.Result is Stream stream)
            {
                return stream.CopyToAsync(response.Body, requestContext.HttpContext.RequestAborted);
            }

            var returnBytes = Encoding.UTF8.GetBytes(requestContext.Result?.ToString() ?? "");
            
            return ReturnByteArray(response, returnBytes);
        }

        private Task ReturnByteArray(HttpResponse response, ReadOnlySpan<byte> bytes)
        {
            response.ContentLength = bytes.Length;

            var responseSpan = response.BodyWriter.GetSpan(bytes.Length);

            bytes.CopyTo(responseSpan);
            
            response.BodyWriter.Advance(bytes.Length);

            return Task.CompletedTask;
        }
    }
}
