using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using EasyRpc.AspNetCore.EndPoints;
using EasyRpc.AspNetCore.Routing;
using Microsoft.AspNetCore.Http;

namespace EasyRpc.AspNetCore.Documentation
{
    public class StaticResourceEndPointMethodHandler : IEndPointMethodHandler
    {
        private readonly IStaticResource _staticResource;

        public StaticResourceEndPointMethodHandler(EndPointServices services, 
            IEndPointMethodConfigurationReadOnly configuration, IStaticResource staticResource)
        {
            Services = services;
            Configuration = configuration;
            _staticResource = staticResource;
        }
        
        /// <inheritdoc />
        public IEndPointMethodConfigurationReadOnly Configuration { get; }

        /// <inheritdoc />
        public EndPointServices Services { get; }

        /// <inheritdoc />
        public IRpcRouteInformation RouteInformation => Configuration.RouteInformation;

        /// <inheritdoc />
        public string HttpMethod => HttpMethods.Get;

        /// <inheritdoc />
        public async Task HandleRequest(HttpContext context)
        {
            context.Response.ContentType = _staticResource.ContentType;

            if (_staticResource.IsBrCompressed)
            {
                context.Response.Headers.TryAdd("ContentEncoding", "br");
            }

            await context.Response.Body.WriteAsync(_staticResource.Content);
        }
    }
}
