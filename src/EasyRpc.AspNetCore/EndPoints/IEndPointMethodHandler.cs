using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using EasyRpc.AspNetCore.Routing;
using Microsoft.AspNetCore.Http;

namespace EasyRpc.AspNetCore.EndPoints
{
    /// <summary>
    /// 
    /// </summary>
    public interface IEndPointMethodHandler
    {
        /// <summary>
        /// Configuration
        /// </summary>
        IEndPointMethodConfigurationReadOnly Configuration { get; }

        /// <summary>
        /// Services associated with this endpoint
        /// </summary>
        EndPointServices Services { get; }

        /// <summary>
        /// Route information for end point
        /// </summary>
        IRpcRouteInformation RouteInformation { get; }
  
        /// <summary>
        /// HTTP method to handle
        /// </summary>
        string HttpMethod { get; }

        /// <summary>
        /// Handles http request to end point
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        Task HandleRequest(HttpContext context);
    }
}
