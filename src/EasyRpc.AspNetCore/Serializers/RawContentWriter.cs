﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

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
        /// <inheritdoc />
        public Task WriteRawContent(RequestExecutionContext requestContext, string contentType, string contentEncoding)
        {
            var response = requestContext.HttpContext.Response;

            response.StatusCode = requestContext.HttpStatusCode;

            response.ContentType = contentType;

            if (contentEncoding != null)
            {
                response.Headers.TryAdd("Content-Encoding", contentEncoding);
            }

            if (requestContext.Result is byte[] bytes)
            {
                response.ContentLength = bytes.Length;

                return response.Body.WriteAsync(bytes, 0, bytes.Length, requestContext.HttpContext.RequestAborted);
            }
            
            if (requestContext.Result is Stream stream)
            {
                return stream.CopyToAsync(response.Body, requestContext.HttpContext.RequestAborted);
            }

            var returnBytes = Encoding.UTF8.GetBytes(requestContext.Result?.ToString() ?? "");

            response.ContentLength = returnBytes.Length;

            return response.Body.WriteAsync(returnBytes, 0, returnBytes.Length, requestContext.HttpContext.RequestAborted);
        }
    }
}
