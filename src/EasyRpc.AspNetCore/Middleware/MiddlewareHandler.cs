using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using EasyRpc.AspNetCore.Configuration;
using EasyRpc.AspNetCore.EndPoints;
using EasyRpc.AspNetCore.Routing;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace EasyRpc.AspNetCore.Middleware
{
    /// <summary>
    /// Middleware handler interface
    /// </summary>
    public interface IMiddlewareHandler
    {
        /// <summary>
        /// Attach api to application pipeline
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="configuration"></param>
        /// <returns></returns>
        IApplicationBuilder Attach(IApplicationBuilder builder, Action<IApiConfiguration> configuration);
    }

    /// <inheritdoc />
    public class MiddlewareHandler : IMiddlewareHandler
    {
        /// <inheritdoc />
        public IApplicationBuilder Attach(IApplicationBuilder builder,  Action<IApiConfiguration> configuration)
        {
            ConfigureEndPoints(builder, configuration);

            return builder;
        }

        private void ConfigureEndPoints(IApplicationBuilder builder, Action<IApiConfiguration> configuration)
        {
            var scope = CreateServiceProvider(builder);

            var apiConfig = ConfigureApi(configuration, scope);

            ConfigureEndPointRouting(builder, apiConfig, scope);

            scope.GetService<EndPointServices>().ConfigurationComplete(scope);
        }

        protected virtual void ConfigureEndPointRouting(IApplicationBuilder builder,
            IInternalApiConfiguration apiConfig, IServiceProvider scope)
        {
            var routingConfiguration = apiConfig.GetCurrentApiInformation().ConfigurationMethods.GetConfiguration<RoutingConfiguration>();
            
            if (routingConfiguration.UseAspNetCoreRouting)
            {
                scope.GetRequiredService<IAspNetRoutingHandler>().Attach(builder, apiConfig, scope);
            }
            else
            {
                scope.GetRequiredService<IInternalRoutingHandler>().Attach(builder, apiConfig, scope);
            }
        }

        protected virtual IInternalApiConfiguration ConfigureApi(Action<IApiConfiguration> configuration, IServiceProvider scope)
        {
            var factory = scope.GetService<IApiConfigurationFactory>();

            var apiConfig = factory.CreateApiConfiguration(scope);

            configuration(apiConfig);

            return apiConfig;
        }

        protected virtual IServiceProvider CreateServiceProvider(IApplicationBuilder builder)
        {
            var scope = builder.ApplicationServices.CreateScope();

            var applicationLifetime = builder.ApplicationServices.GetRequiredService<IHostApplicationLifetime>();

            applicationLifetime.ApplicationStopped.Register(() => scope.Dispose());

            return scope.ServiceProvider;
        }
    }
}
