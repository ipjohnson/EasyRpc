using System;
using System.Threading.Tasks;
using EasyRpc.AspNetCore.Content;
using EasyRpc.AspNetCore.Converters;
using EasyRpc.AspNetCore.Documentation;
using EasyRpc.AspNetCore.Middleware;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Newtonsoft.Json;

namespace EasyRpc.AspNetCore
{
    /// <summary>
    /// Application builder extensions
    /// </summary>
    public static class AppBuilderExtensions
    {
        /// <summary>
        /// Add Easy RPC dependency injection configuration, this is required
        /// </summary>
        /// <param name="collection"></param>
        /// <param name="configuration"></param>
        public static IServiceCollection AddJsonRpc(this IServiceCollection collection, Action<RpcServiceConfiguration> configuration = null)
        {
            collection.TryAddTransient<IRpcMessageProcessor, RpcMessageProcessor>();
            collection.TryAddTransient<IContentSerializer, DefaultJsonContentSerializer>();
            collection.TryAddTransient<IContentSerializerProvider, ContentSerializerProvider>();
            collection.TryAddTransient<IContentEncodingProvider, ContentEncodingProvider>();
            collection.TryAddTransient<IExposeMethodInformationCacheManager, ExposeMethodInformationCacheManager>();
            collection.TryAddSingleton<IParameterArrayDeserializerBuilder, ParameterArrayDeserializerBuilder>();
            collection.TryAddSingleton<INamedParameterDeserializerBuilder, NamedParameterDeserializerBuilder>();
            collection.TryAddSingleton<IArrayMethodInvokerBuilder, ArrayMethodInvokerBuilder>();
            collection.TryAddSingleton<IInstanceActivator, InstanceActivator>();
            collection.TryAddSingleton<IFromServicesManager, FromServicesManager>();
            collection.TryAddSingleton<JsonSerializer>();

            // documentation
            collection.TryAddSingleton<IXmlDocumentationProvider, XmlDocumentationProvider>();
            collection.TryAddTransient<IDocumentationRequestProcessor, DocumentationRequestProcessor>();
            collection.TryAddTransient<IWebAssetProvider, WebAssetProvider>();
            collection.TryAddTransient<IMethodPackageMetadataCreator, MethodPackageMetadataCreator>();
            collection.TryAddTransient<IVariableReplacementService, VariableReplacementService>();
            collection.TryAddTransient<IReplacementValueProvider, ReplacementValueProvider>();
            collection.TryAddTransient<ITypeDefinitionPackageProvider, TypeDefinitionPackageProvider>();

            collection.Configure(configuration ?? (option => { }));

            return collection;
        }

        /// <summary>
        /// Adds JSON-RPC 2.0 support
        /// </summary>
        /// <param name="appBuilder">app builder</param>
        /// <param name="basePath">base path for api</param>
        /// <param name="configure">configure api</param>
        public static IApplicationBuilder UseJsonRpc(this IApplicationBuilder appBuilder, string basePath,
            Action<IApiConfiguration> configure)
        {
            EasyRpcMiddleware.AttachMiddleware(appBuilder, basePath, configure);

            return appBuilder;
        }

        /// <summary>
        /// Redirects all requests to service api for documentation
        /// </summary>
        /// <param name="appBuilder"></param>
        /// <param name="basePath"></param>
        /// <returns></returns>
        public static IApplicationBuilder RedirectToDocumentation(this IApplicationBuilder appBuilder, string basePath)
        {
            appBuilder.Use((context, next) =>
            {
                var response = context.Response;
                var redirectPath = context.Request.PathBase.Value + basePath;

                if (context.Request.Path.Value?.EndsWith("/favicon.ico") ?? false)
                {
                    response.Headers["Location"] = redirectPath + "favicon.ico";
                }
                else
                {
                    response.Headers["Location"] = redirectPath;
                }

                response.StatusCode = 301;

                return Task.CompletedTask;
            });

            return appBuilder;
        }
    }
}
