using System;
using System.Linq;
using System.Reflection;

namespace EasyRpc.AspNetCore
{
    /// <summary>
    /// C# extension class for IAPiConfiguration
    /// </summary>
    public static class ApiConfigurationExtensions
    {
        /// <summary>
        /// Expose an assembly containing a specific type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="configuration"></param>
        /// <returns></returns>
        public static ITypeSetExposureConfiguration ExposeAssemblyContaining<T>(this IApiConfiguration configuration)
        {
            if (configuration == null) throw new ArgumentNullException(nameof(configuration));

            return configuration.Expose(typeof(T).GetTypeInfo().Assembly.ExportedTypes);
        }

        /// <summary>
        /// Expose namespace containing a specific type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="configuration"></param>
        /// <returns></returns>
        public static ITypeSetExposureConfiguration ExposeNamespaceContaining<T>(this IApiConfiguration configuration)
        {
            if (configuration == null) throw new ArgumentNullException(nameof(configuration));

            var namespaceT = typeof(T).Namespace;

            return configuration.Expose(typeof(T).GetTypeInfo().Assembly.ExportedTypes.Where(t => t.Namespace.StartsWith(namespaceT)));
        }
    }
}
