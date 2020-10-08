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
        private StaticResource _staticResource;

        public StaticResourceEndPointMethodHandler(EndPointServices services, IEndPointMethodConfigurationReadOnly configuration, StaticResource staticResource)
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
        public Task HandleRequest(HttpContext context)
        {
            throw new NotImplementedException();
        }
    }
}
