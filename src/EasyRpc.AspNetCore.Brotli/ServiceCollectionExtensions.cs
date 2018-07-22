using System;
using System.Collections.Generic;
using System.Text;
using EasyRpc.AspNetCore.Content;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace EasyRpc.AspNetCore.Brotli
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Add brotli support for rpc calls.
        /// </summary>
        /// <param name="serviceCollection"></param>
        /// <returns></returns>
        public static IServiceCollection AddBrotliRcpSupport(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddSingleton<IContentEncoder, BrotliContentEncoder>();

            return serviceCollection;
        }
    }
}
