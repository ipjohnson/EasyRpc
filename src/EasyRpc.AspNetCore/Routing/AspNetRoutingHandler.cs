using System;
using System.Collections.Generic;
using System.Text;
using EasyRpc.AspNetCore.Configuration;
using Microsoft.AspNetCore.Builder;

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
        void Attach(IApplicationBuilder builder, IInternalApiConfiguration apiConfig, IServiceProvider scopedProvider);
    }

    /// <inheritdoc />
    public class AspNetRoutingHandler : IAspNetRoutingHandler
    {

        /// <inheritdoc />
        public void Attach(IApplicationBuilder builder, IInternalApiConfiguration apiConfig,
            IServiceProvider scopedProvider)
        {
            var endPointList = apiConfig.GetEndPointHandlers();
            
            builder.UseEndpoints(routeBuilder =>
            {
                foreach (var methodHandler in endPointList)
                {
                    routeBuilder.MapMethods(methodHandler.RouteInformation.RouteTemplate,
                        new[] {methodHandler.HttpMethod}, methodHandler.HandleRequest);
                }
            });
        }
    }
}
