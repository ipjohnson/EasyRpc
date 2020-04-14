using System;
using System.Collections.Generic;
using System.Text;
using EasyRpc.AspNetCore.Configuration;
using Microsoft.AspNetCore.Builder;

namespace EasyRpc.AspNetCore.Routing
{
    public interface IAspNetRoutingHandler
    {
        void Attach(IApplicationBuilder builder, IInternalApiConfiguration apiConfig, IServiceProvider scopedProvider);
    }

    public class AspNetRoutingHandler : IAspNetRoutingHandler
    {
        public void Attach(IApplicationBuilder builder, IInternalApiConfiguration apiConfig,
            IServiceProvider scopedProvider)
        {
            var endPointList = apiConfig.GetEndPointHandlers();

            builder.UseRouting();
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
