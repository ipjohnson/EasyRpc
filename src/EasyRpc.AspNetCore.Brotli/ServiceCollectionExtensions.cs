using EasyRpc.AspNetCore.Content;
using Microsoft.Extensions.DependencyInjection;

namespace EasyRpc.AspNetCore.Brotli
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Add brotli support for rpc calls.
        /// </summary>
        /// <param name="serviceCollection"></param>
        /// <returns></returns>
        public static IServiceCollection AddBrotliRpcSupport(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddSingleton<IContentEncoder, BrotliContentEncoder>();

            return serviceCollection;
        }
    }
}
