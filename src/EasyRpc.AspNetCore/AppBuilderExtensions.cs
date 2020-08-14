using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using EasyRpc.AspNetCore.Authorization;
using EasyRpc.AspNetCore.Configuration;
using EasyRpc.AspNetCore.Documentation;
using EasyRpc.AspNetCore.EndPoints;
using EasyRpc.AspNetCore.Errors;
using EasyRpc.AspNetCore.CodeGeneration;
using EasyRpc.AspNetCore.Middleware;
using EasyRpc.AspNetCore.ModelBinding;
using EasyRpc.AspNetCore.ModelBinding.AspNetRouting;
using EasyRpc.AspNetCore.ModelBinding.InternalRouting;
using EasyRpc.AspNetCore.Routing;
using EasyRpc.AspNetCore.Serializers;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace EasyRpc.AspNetCore
{
    /// <summary>
    /// C# extension methods for app building
    /// </summary>
    public static class AppBuilderExtensions
    {
        /// <summary>
        /// Add rpc services to service collection
        /// </summary>
        /// <param name="serviceCollection"></param>
        /// <param name="registerJsonSerializer"></param>
        /// <returns></returns>
        public static IServiceCollection AddRpcServices(this IServiceCollection serviceCollection, bool registerJsonSerializer = true)
        {
            serviceCollection.TryAddTransient<IMiddlewareHandler, MiddlewareHandler>();

            serviceCollection.TryAddScoped<ICustomActionResultExecutor, CustomActionResultExecutor>();
            serviceCollection.TryAddScoped<IApiConfigurationFactory, ApiConfigurationFactory>();
            serviceCollection.TryAddScoped<IEndPointRouteBuilder, EndPointRouteBuilder>();
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
            serviceCollection.TryAddScoped<IApiConfigurationFactory, ApiConfigurationFactory>();
            serviceCollection.TryAddScoped<IApplicationConfigurationService, ApplicationConfigurationService>();
            serviceCollection.TryAddScoped<IInternalRoutingHandler, InternalRoutingHandler>();
            serviceCollection.TryAddScoped<IAspNetRoutingHandler, AspNetRoutingHandler>();
            serviceCollection.TryAddScoped<IConfigurationManager, ConfigurationManager>();
            serviceCollection.TryAddScoped<IAuthorizationImplementationProvider, AuthorizationImplementationProvider>();
            serviceCollection.TryAddScoped<IRegisteredEndPoints, RegisteredEndPoints>();
            serviceCollection.TryAddScoped<IDocumentationService, DocumentationService>();
            serviceCollection.TryAddScoped<IOpenApiGenerationService, OpenApiGenerationService>();
            serviceCollection.TryAddScoped<IOpenApiSchemaGenerator, OpenApiSchemaGenerator>();
            serviceCollection.TryAddScoped<IKnownOpenApiTypeMapper, KnownOpenApiTypeMapper>();
            serviceCollection.TryAddScoped<ISwaggerAssetProvider, SwaggerAssetProvider>();
            serviceCollection.TryAddScoped<INoBodyParameterBinderDelegateBuilder, NoBodyParameterBinderDelegateBuilder>();
            serviceCollection.TryAddScoped<IBodyParameterBinderDelegateBuilder, BodyParameterBinderDelegateBuilder>();
            serviceCollection.TryAddScoped<IBodySingleParameterBinderDelegateBuilder, BodySingleParameterBinderDelegateBuilder>();
            serviceCollection.TryAddScoped<IStringValueModelBinder, StringValueModelBinder>();
            serviceCollection.TryAddScoped<IInternalRoutingParameterBinder, InternalRoutingParameterBinder>();
            serviceCollection.TryAddScoped<IAspNetRoutingParameterBinder, AspNetRoutingParameterBinder>();
            serviceCollection.TryAddScoped<ISpecialParameterBinder, SpecialParameterBinder>();
            serviceCollection.TryAddScoped<IEnvironmentConfiguration, EnvironmentConfiguration>();
            serviceCollection.TryAddScoped<IWrappedResultTypeCreator, WrappedResultTypeCreator>();

            serviceCollection.TryAddScoped<BaseEndPointServices>();

            if (registerJsonSerializer)
            {
                serviceCollection.TryAddScoped<IContentSerializer, JsonContentSerializer>();
            }

            return serviceCollection;
        }

        /// <summary>
        /// Add rpc services to asp.net pipeline
        /// </summary>
        /// <param name="appBuilder">application builder</param>
        /// <param name="configure">api configuration</param>
        /// <returns></returns>
        public static IApplicationBuilder UseRpcServices(this IApplicationBuilder appBuilder,
            Action<IApiConfiguration> configure = null)
        {
            var middlewareHandler = appBuilder.ApplicationServices.GetService<IMiddlewareHandler>();

            if (middlewareHandler == null)
            {
                throw new Exception("Please add services.AddRpcServices(); in the ConfigureServices method of your Startup.cs file.");
            }

            if (configure == null)
            {
                configure = DefaultAction;
            }

            return middlewareHandler.Attach(appBuilder, configure);
        }

        /// <summary>
        /// Default action when calling UseRpcServices
        /// </summary>
        public static Action<IApiConfiguration> DefaultAction { get; set; } =
            api => api.Expose(Assembly.GetEntryAssembly().ExportedTypes).OnlyAttributed();
    }
}
