using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using EasyRpc.AspNetCore.Routing;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;

namespace EasyRpc.AspNetCore.EndPoints
{
    public class HeadEndPointMethodHandler : IEndPointMethodHandler
    {
        private readonly IEndPointMethodHandler _getMethodHandler;

        public HeadEndPointMethodHandler(IEndPointMethodHandler getMethodHandler, IEndPointMethodConfigurationReadOnly configuration)
        {
            _getMethodHandler = getMethodHandler;
            Configuration = configuration;
        }

        public IEndPointMethodConfigurationReadOnly Configuration { get; }

        public EndPointServices Services => _getMethodHandler.Services;

        public IRpcRouteInformation RouteInformation => Configuration.RouteInformation;

        public string HttpMethod => HttpMethods.Head;

        public Task HandleRequest(HttpContext context)
        {
            context.Features.Set<IHttpResponseBodyFeature>(new StreamResponseBodyFeature(Stream.Null));

            return _getMethodHandler.HandleRequest(context);
        }
    }
}
