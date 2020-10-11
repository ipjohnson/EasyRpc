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
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace EasyRpc.AspNetCore.Middleware
{
    /// <summary>
    /// Middleware handler interface
    /// </summary>
    public interface IMiddlewareHandler
    {
        IEndpointRouteBuilder Attach(IEndpointRouteBuilder builder, Action<IRpcApi> configuration);

        /// <summary>
        /// Attach api to application pipeline
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="configuration"></param>
        /// <returns></returns>
        IApplicationBuilder Attach(IApplicationBuilder builder, Action<IRpcApi> configuration);
    }

    /// <inheritdoc />
    public class MiddlewareHandler : IMiddlewareHandler
    {
        public IEndpointRouteBuilder Attach(IEndpointRouteBuilder builder, Action<IRpcApi> configuration)
        {
            var scope = CreateServiceProvider(builder.ServiceProvider);

            var apiConfig = ConfigureApi(configuration, scope);

            scope.GetRequiredService<IAspNetRoutingHandler>().Attach(builder, apiConfig, scope);

            apiConfig.GetCurrentApiInformation().ConfigurationMethods.GetConfiguration<RoutingConfiguration>().UseAspNetCoreRouting = true;

            scope.GetService<EndPointServices>().ConfigurationComplete(scope);

            return builder;
        }

        /// <inheritdoc />
        public IApplicationBuilder Attach(IApplicationBuilder builder,  Action<IRpcApi> configuration)
        {
            ConfigureEndPoints(builder, configuration);

            return builder;
        }

        private void ConfigureEndPoints(IApplicationBuilder builder, Action<IRpcApi> configuration)
        {
            var scope = CreateServiceProvider(builder.ApplicationServices);

            var apiConfig = ConfigureApi(configuration, scope);

            scope.GetRequiredService<IInternalRoutingHandler>().Attach(builder, apiConfig, scope);

            apiConfig.GetCurrentApiInformation().ConfigurationMethods.GetConfiguration<RoutingConfiguration>().UseAspNetCoreRouting = false;

            scope.GetService<EndPointServices>().ConfigurationComplete(scope);
        }
        
        protected virtual IInternalApiConfiguration ConfigureApi(Action<IRpcApi> configuration, IServiceProvider scope)
        {
            var factory = scope.GetService<IApiConfigurationFactory>();

            var apiConfig = factory.CreateApiConfiguration(scope);

            configuration(apiConfig);

            return apiConfig;
        }

        protected virtual IServiceProvider CreateServiceProvider(IServiceProvider applicationServiceProvider)
        {
            var scope = applicationServiceProvider.CreateScope();

            var applicationLifetime = applicationServiceProvider.GetRequiredService<IHostApplicationLifetime>();

            applicationLifetime.ApplicationStopped.Register(() => scope.Dispose());

            return scope.ServiceProvider;
        }
    }
}
