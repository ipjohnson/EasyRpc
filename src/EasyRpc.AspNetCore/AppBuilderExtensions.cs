using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EasyRpc.AspNetCore.Middleware;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

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
            collection.TryAddSingleton<IJsonRpcMessageProcessor, JsonRpcMessageProcessor>();
            collection.TryAddSingleton<IJsonSerializerProvider, JsonSerializerProvider>();
            collection.TryAddSingleton<IOrderedParameterMethodInvokeBuilder, OrderedParameterMethodInvokeBuilder>();
            collection.TryAddSingleton<INamedParameterMethodInvokerBuilder, NamedParameterMethodInvokerBuilder>();

            collection.TryAddSingleton<IRpcHeaderContext, RpcHeaderContext>();
            collection.TryAddSingleton<IHttpContextAccessor,HttpContextAccessor>();
            
            collection.Configure(configuration ?? (option => { }));

            return collection;
        }

        /// <summary>
        /// Adds JSON-RPC support
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
