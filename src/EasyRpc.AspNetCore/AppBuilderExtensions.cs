using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using EasyRpc.Abstractions.Path;
using EasyRpc.AspNetCore.Authorization;
using EasyRpc.AspNetCore.Configuration;
using EasyRpc.AspNetCore.Documentation;
using EasyRpc.AspNetCore.EndPoints;
using EasyRpc.AspNetCore.Errors;
using EasyRpc.AspNetCore.CodeGeneration;
using EasyRpc.AspNetCore.ContentEncoding;
using EasyRpc.AspNetCore.Middleware;
using EasyRpc.AspNetCore.ModelBinding;
using EasyRpc.AspNetCore.ModelBinding.AspNetRouting;
using EasyRpc.AspNetCore.ModelBinding.InternalRouting;
using EasyRpc.AspNetCore.Routing;
using EasyRpc.AspNetCore.Serializers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace EasyRpc.AspNetCore
{
    /// <summary>
    /// Default rpc services
    /// </summary>
    public class DefaultRpcServices
    {
        /// <summary>
        /// Register default System.Text.Json serializer
        /// </summary>
        public bool RegisterJsonSerializer { get; set; } = true;

        /// <summary>
        /// Register XmlSerializer 
        /// </summary>
        public bool RegisterXmlSerializer { get; set; } = false;

        /// <summary>
        /// Register default OPTIONS handler
        /// </summary>
        public bool RegisterDefaultOptionsHandler { get; set; } = false;

        /// <summary>
        /// Register default HEAD handler
        /// </summary>
        public bool RegisterDefaultHeadHandler { get; set; } = false;
    }

    /// <summary>
    /// C# extension methods for app building
    /// </summary>
    public static class AppBuilderExtensions
    {
        /// <summary>
        /// Add rpc services to service collection
        /// </summary>
        /// <param name="serviceCollection"></param>
        /// <param name="configurationAction"></param>
        /// <returns></returns>
        public static IServiceCollection AddRpcServices(this IServiceCollection serviceCollection, Action<DefaultRpcServices> configurationAction = null)
        {
            var config = new DefaultRpcServices();

            configurationAction?.Invoke(config);

            serviceCollection.TryAddTransient<IMiddlewareHandler, MiddlewareHandler>();

            serviceCollection.TryAddScoped<ICustomActionResultExecutor, CustomActionResultExecutor>();
            serviceCollection.TryAddScoped<IApiConfigurationFactory, ApiConfigurationFactory>();
            serviceCollection.TryAddScoped<IInternalEndPointRouteBuilder, InternalEndPointRouteBuilder>();
            serviceCollection.TryAddScoped<IContentSerializationService, ContentSerializationService>();
            serviceCollection.TryAddScoped<IResponseDelegateCreator, ResponseDelegateCreator>();
            serviceCollection.TryAddScoped<IUnmappedEndPointHandler, UnmappedEndPointHandler>();
            serviceCollection.TryAddScoped<IEndPointAuthorizationService, EndPointAuthorizationService>();
            serviceCollection.TryAddScoped<IParameterBinderDelegateBuilder, ParameterBinderDelegateBuilder>();
            serviceCollection.TryAddScoped<IDeserializationTypeCreator, DeserializationTypeCreator>();
            serviceCollection.TryAddScoped<IMethodInvokerCreationService, MethodInvokerCreationService>();
            serviceCollection.TryAddScoped<IErrorHandler, DefaultErrorHandler>();
            serviceCollection.TryAddScoped<IErrorWrappingService, ErrorWrappingService>();
            serviceCollection.TryAddScoped<IErrorResultTypeCreator, ErrorResultTypeCreator>();
            serviceCollection.TryAddScoped<IRawContentWriter, RawContentWriter>();
            serviceCollection.TryAddScoped<ICompressionActionProvider,DefaultCompressionPredicateProvider>();
            serviceCollection.TryAddScoped<ICompressionSelectorService,CompressionSelectorService>();
            serviceCollection.TryAddScoped<IApiConfigurationFactory, ApiConfigurationFactory>();
            serviceCollection.TryAddScoped<IApplicationConfigurationService, ApplicationConfigurationService>();
            serviceCollection.TryAddScoped<IInternalRoutingHandler, InternalRoutingHandler>();
            serviceCollection.TryAddScoped<IAspNetRoutingHandler, AspNetRoutingHandler>();
            serviceCollection.TryAddScoped<IConfigurationManager, ConfigurationManager>();
            serviceCollection.TryAddScoped<IAuthorizationImplementationProvider, AuthorizationImplementationProvider>();
            serviceCollection.TryAddScoped<IRegisteredEndPoints, RegisteredEndPoints>();

            serviceCollection.TryAddScoped<IOpenApiGenerationService, OpenApiGenerationService>();
            serviceCollection.TryAddScoped<IOpenApiSchemaGenerator, OpenApiSchemaGenerator>();
            serviceCollection.TryAddScoped<IKnownOpenApiTypeMapper, KnownOpenApiTypeMapper>();
            serviceCollection.TryAddScoped<ISwaggerStaticResourceProvider, SwaggerStaticResourceProvider>();
            serviceCollection.TryAddScoped<IStringTokenReplacementService, StringTokenReplacementService>();
            serviceCollection.TryAddScoped<ITokenValueProvider, TokenValueProvider>();
            serviceCollection.TryAddScoped<INoBodyParameterBinderDelegateBuilder, NoBodyParameterBinderDelegateBuilder>();
            serviceCollection.TryAddScoped<IBodyParameterBinderDelegateBuilder, BodyParameterBinderDelegateBuilder>();
            serviceCollection.TryAddScoped<IBodySingleParameterBinderDelegateBuilder, BodySingleParameterBinderDelegateBuilder>();
            serviceCollection.TryAddScoped<IStringValueModelBinder, StringValueModelBinder>();
            serviceCollection.TryAddScoped<IInternalRoutingParameterBinder, InternalRoutingParameterBinder>();
            serviceCollection.TryAddScoped<IAspNetRoutingParameterBinder, AspNetRoutingParameterBinder>();
            serviceCollection.TryAddScoped<ISpecialParameterBinder, SpecialParameterBinder>();
            serviceCollection.TryAddScoped<IEnvironmentConfiguration, EnvironmentConfiguration>();
            serviceCollection.TryAddScoped<IWrappedResultTypeCreator, WrappedResultTypeCreator>();
            serviceCollection.TryAddScoped<IXmlDocProvider,XmlDocProvider>();

            serviceCollection.TryAddScoped<EndPointServices>();

            if (config.RegisterXmlSerializer)
            {
                serviceCollection.AddScoped<IContentSerializer, XmlContentSerializer>();
                serviceCollection.AddScoped<ISerializationTypeAttributor, XmlContentSerializerTypeAttributor>();
            }

            if (config.RegisterJsonSerializer)
            {
                serviceCollection.AddScoped<IContentSerializer, JsonContentSerializer>();
            }

            if(config.RegisterDefaultHeadHandler)
            {
                serviceCollection.AddScoped<IApiEndPointInspector, HeadEndPointInspector>();
            }

            if (config.RegisterDefaultOptionsHandler)
            {
                serviceCollection.AddScoped<IDefaultHttpMethodHandler, OptionsEndPointHandler>();
            }

            serviceCollection.AddScoped<IApiEndPointInspector, DocumentationEndPointInspector>();

            return serviceCollection;
        }

        public static IEndpointRouteBuilder MapRpcApi(this IEndpointRouteBuilder routeBuilder,
            Action<IRpcApi> configure = null)
        {
            var middlewareHandler = routeBuilder.ServiceProvider.GetService<IMiddlewareHandler>();

            if (middlewareHandler == null)
            {
                throw new Exception("Please add services.AddRpcServices(); in the ConfigureServices method of your Startup.cs file.");
            }

            return middlewareHandler.Attach(routeBuilder, configure ?? DefaultAction);
        }

        /// <summary>
        /// Add rpc services to asp.net pipeline
        /// </summary>
        /// <param name="appBuilder">application builder</param>
        /// <param name="configure">api configuration</param>
        /// <returns></returns>
        public static IApplicationBuilder UseRpcRouting(this IApplicationBuilder appBuilder,
            Action<IRpcApi> configure = null)
        {
            var middlewareHandler = appBuilder.ApplicationServices.GetService<IMiddlewareHandler>();

            if (middlewareHandler == null)
            {
                throw new Exception("Please add services.AddRpcServices(); in the ConfigureServices method of your Startup.cs file.");
            }

            return middlewareHandler.Attach(appBuilder, configure ?? DefaultAction);
        }

        /// <summary>
        /// Default action when calling UseRpcServices
        /// </summary>
        public static Action<IRpcApi> DefaultAction { get; set; } =
            api => api.Expose(Assembly.GetEntryAssembly().ExportedTypes).OnlyAttributed();


        public static void DefaultConfigurationAction(IRpcApi api)
        {
            api.ExposeModules();

            api.Expose(Assembly.GetEntryAssembly().ExportedTypes).OnlyAttributed();
        }
    }
}
