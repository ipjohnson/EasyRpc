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
    public class RequestExecutionContext
    {
        private bool _continueRequest;

        public RequestExecutionContext(HttpContext httpContext, IEndPointMethodHandler handler, int httpStatusCode)
        {
            _continueRequest = true;
            HttpContext = httpContext;
            Handler = handler;
            HttpStatusCode = httpStatusCode;
        }

        public List<IRequestFilter> CallFilters { get; set; }

        public object ServiceInstance { get; set; }

        public HttpContext HttpContext { get; }

        public IRequestParameters Parameters { get; set; }

        public bool ContinueRequest
        {
            get => _continueRequest && !HttpContext.RequestAborted.IsCancellationRequested;
            set => _continueRequest = value;
        }

        public bool ResponseHasStarted => HttpContext.Response.HasStarted;

        public IEndPointMethodHandler Handler { get; }

        public object Result { get; set; }

        public IContentSerializer ContentSerializer { get; set; }

        public bool CanCompress { get; set; }

        public int HttpStatusCode { get; set; }
    }
}
