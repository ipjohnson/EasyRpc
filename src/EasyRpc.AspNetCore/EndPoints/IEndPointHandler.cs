using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace EasyRpc.AspNetCore.EndPoints
{
    /// <summary>
    /// End point handler
    /// </summary>
    public interface IEndPointHandler
    {
        /// <summary>
        /// Path for this 
        /// </summary>
        string Path { get; }

        /// <summary>
        /// True if the handler supports /service/method/argValue
        /// </summary>
        bool SupportsLongerPaths { get; }

        /// <summary>
        /// Handles http request to end point
        /// </summary>
        /// <param name="context"></param>
        /// <param name="next"></param>
        /// <returns></returns>
        Task HandleRequest(HttpContext context, RequestDelegate next);
    }
}
