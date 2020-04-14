using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using EasyRpc.AspNetCore.Routing;
using Microsoft.AspNetCore.Http;

namespace EasyRpc.AspNetCore.EndPoints
{
    public interface IEndPointMethodHandler
    {
        EndPointMethodConfiguration Configuration { get; }

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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="serializerId"></param>
        /// <returns></returns>
        object GetSerializerData(int serializerId);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="serializerId"></param>
        /// <param name="data"></param>
        void SetSerializerData(int serializerId, object data);
    }
}
