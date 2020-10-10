using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using EasyRpc.AspNetCore.EndPoints;
using EasyRpc.AspNetCore.Routing;
using Microsoft.AspNetCore.Http;

namespace EasyRpc.AspNetCore.Documentation
{
    public class OpenApiJsonEndPointMethodHandler : IEndPointMethodHandler
    {
        private readonly IOpenApiGenerationService _openApiGenerationService;

        public OpenApiJsonEndPointMethodHandler(EndPointServices services, 
            IEndPointMethodConfigurationReadOnly configuration, 
            IOpenApiGenerationService openApiGenerationService)
        {
            Services = services;
            Configuration = configuration;
            _openApiGenerationService = openApiGenerationService;
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
            return _openApiGenerationService.Execute(context);
        }
    }
}
