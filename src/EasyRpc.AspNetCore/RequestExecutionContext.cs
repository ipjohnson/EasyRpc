using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EasyRpc.AspNetCore.EndPoints;
using EasyRpc.AspNetCore.Filters;
using EasyRpc.AspNetCore.Serializers;
using Microsoft.AspNetCore.Http;

namespace EasyRpc.AspNetCore
{
    /// <summary>
    /// Context created for every request
    /// </summary>
    public class RequestExecutionContext
    {
        private bool _continueRequest;

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="httpContext"></param>
        /// <param name="handler"></param>
        /// <param name="httpStatusCode"></param>
        public RequestExecutionContext(HttpContext httpContext, IEndPointMethodHandler handler, int httpStatusCode)
        {
            _continueRequest = true;
            HttpContext = httpContext;
            Handler = handler;
            HttpStatusCode = httpStatusCode;
        }

        /// <summary>
        /// List of call filters for the request, can be null
        /// </summary>
        public List<IRequestFilter> CallFilters { get; set; }

        /// <summary>
        /// Service instance when available 
        /// </summary>
        public object ServiceInstance { get; set; }

        /// <summary>
        ///  HttpContext associated with request
        /// </summary>
        public HttpContext HttpContext { get; }

        /// <summary>
        /// Parameters for rpc call
        /// </summary>
        public IRequestParameters Parameters { get; set; }

        /// <summary>
        /// Continue request
        /// </summary>
        public bool ContinueRequest
        {
            get => _continueRequest && !HttpContext.RequestAborted.IsCancellationRequested;
            set => _continueRequest = value;
        }

        /// <summary>
        /// Response has started based on http context
        /// </summary>
        public bool ResponseHasStarted => HttpContext.Response.HasStarted;
        
        /// <summary>
        /// handle method for request
        /// </summary>
        public IEndPointMethodHandler Handler { get; }
        
        /// <summary>
        /// Result property to be returned to client
        /// </summary>
        public object Result { get; set; }

        /// <summary>
        /// Content serializer if available 
        /// </summary>
        public IContentSerializer ContentSerializer { get; set; }
        
        /// <summary>
        /// can call request be compressed
        /// </summary>
        public bool CanCompress { get; set; }

        /// <summary>
        /// Http success status
        /// </summary>
        public int HttpStatusCode { get; set; }
    }
}
