using System;
using System.Collections.Generic;
using System.Text;
using EasyRpc.AspNetCore.Configuration;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace EasyRpc.AspNetCore.Routing
{
    /// <summary>
    /// ASP.Net routing handler
    /// </summary>
    public interface IAspNetRoutingHandler
    {
        /// <summary>
        /// Attach asp.net routing handler to asp.net pipeline
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="apiConfig"></param>
        /// <param name="scopedProvider"></param>
        void Attach(IEndpointRouteBuilder builder, IInternalApiConfiguration apiConfig, IServiceProvider scopedProvider);
    }

    /// <inheritdoc />
    public class AspNetRoutingHandler : IAspNetRoutingHandler
    {
        /// <inheritdoc />
        public void Attach(IEndpointRouteBuilder routeBuilder, IInternalApiConfiguration apiConfig,
            IServiceProvider scopedProvider)
        {
            var endPointList = apiConfig.GetEndPointHandlers();

            foreach (var methodHandlerPair in endPointList)
            {
                foreach (var methodHandler in methodHandlerPair.Value.Values)
                {
                    routeBuilder.MapMethods(methodHandler.RouteInformation.RouteTemplate,
                        new[] { methodHandler.HttpMethod }, methodHandler.HandleRequest);
                }
            }
        }
    }
}
