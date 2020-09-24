using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;

namespace EasyRpc.AspNetCore.EndPoints
{
    public class HeadEndPointHandler : IDefaultHttpMethodHandler
    {
        public bool CanHandle(HttpContext context, bool isMatched, IEndPointMethodHandler[] handlers)
        {
            if (!isMatched)
            {
                return false;
            }

            return context.Request.Method == HttpMethods.Head && 
                handlers.Any(handler => handler.HttpMethod == HttpMethods.Get);
        }

        public Task HandleUnmatched(HttpContext context, RequestDelegate next)
        {
            return next(context);
        }

        public Task HandleMatchedPath(HttpContext httpContext, RequestDelegate next, IEndPointMethodHandler[] handlers)
        {
            var endPointHandler = handlers.FirstOrDefault(handler => handler.HttpMethod == HttpMethods.Get);

            if (endPointHandler == null)
            {
                return next(httpContext);
            }

            httpContext.Features.Set<IHttpResponseBodyFeature>(new StreamResponseBodyFeature(Stream.Null));

            return endPointHandler.HandleRequest(httpContext);
        }
    }
}
