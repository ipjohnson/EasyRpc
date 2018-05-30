using System;
using EasyRpc.AspNetCore.Documentation;
using EasyRpc.AspNetCore.Middleware;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Newtonsoft.Json;

namespace EasyRpc.AspNetCore
{
    public static class AppBuilderExtensions
    {
        /// <summary>
        /// Add Easy RPC dependency injection configuration, this is usually only needed if you want to override
        /// </summary>
        /// <param name="collection"></param>
        /// <param name="configuration"></param>
        public static IServiceCollection AddJsonRpc(this IServiceCollection collection, Action<RpcServiceConfiguration> configuration = null )
        {
            collection.TryAddTransient<IJsonRpcMessageProcessor, JsonRpcMessageProcessor>();
            collection.TryAddSingleton<IJsonSerializerProvider, JsonSerializerProvider>();
            collection.TryAddSingleton<INamedParameterToArrayDelegateProvider,NamedParameterToArrayDelegateProvider>();
            collection.TryAddSingleton<IOrderedParameterToArrayDelegateProvider, OrderedParameterToArrayDelegateProvider>();
            collection.TryAddSingleton<IArrayMethodInvokerBuilder, ArrayMethodInvokerBuilder>();

            collection.TryAddSingleton<IRpcHeaderContext, RpcHeaderContext>();
            collection.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            collection.TryAddSingleton(new JsonSerializer());

            collection.TryAddSingleton<IDocumentationRequestProcessor,DocumentationRequestProcessor>();
            collection.TryAddSingleton<IWebAssetProvider, WebAssetProvider>();
            collection.TryAddSingleton<IMethodPackageMetadataCreator, MethodPackageMetadataCreator>();

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
            JsonRpcMiddleware.AttachMiddleware(appBuilder, basePath, configure);
            
            return appBuilder;
        }
    }
}
