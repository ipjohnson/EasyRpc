using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace EasyRpc.AspNetCore.EndPoints
{
    public interface IDefaultHttpMethodHandler
    {
        bool CanHandle(HttpContext context, bool isMatched, IEndPointMethodHandler[] handlers);

        Task HandleUnmatched(HttpContext context, RequestDelegate next);

        Task HandleMatchedPath(HttpContext httpContext, RequestDelegate next, IEndPointMethodHandler[] handlers);
    }
}
