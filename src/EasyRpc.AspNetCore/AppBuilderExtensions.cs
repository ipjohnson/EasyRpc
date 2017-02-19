using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EasyRpc.AspNetCore.Middleware;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace EasyRpc.AspNetCore
{
    public static class AppBuilderExtensions
    {
        /// <summary>
        /// Add Easy RPC dependency injection configuration, this is usually only needed if you want to override
        /// </summary>
        /// <param name="collection"></param>
        public static IServiceCollection AddJsonRpc(this IServiceCollection collection)
        {
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
