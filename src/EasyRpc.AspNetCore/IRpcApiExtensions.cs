using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using EasyRpc.AspNetCore.Configuration;
using EasyRpc.AspNetCore.Features;

namespace EasyRpc.AspNetCore
{
    /// <summary>
    /// static extension class for IApiConfiguration
    /// </summary>
    public static class IRpcApiExtensions
    {
        /// <summary>
        /// Expose types in the same assembly and namespace as T
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="configuration"></param>
        /// <returns></returns>
        public static  ITypeSetExposureConfiguration ExposeNamespaceContaining<T>(this IRpcApi configuration)
        {
            if (configuration == null) throw new ArgumentNullException(nameof(configuration));

            return configuration.Expose(typeof(T).GetTypeInfo().Assembly.ExportedTypes.Where(TypesThat.AreInTheSameNamespaceAs<T>()));
        }

        /// <summary>
        /// Adds IRequestExecutionContextFeature to HttpContext.
        /// </summary>
        /// <param name="configuration"></param>
        /// <returns></returns>
        public static IApiConfiguration UseRequestExecutionContextFeature(this IApiConfiguration configuration)
        {
            configuration.Configure.Action<RequestExecutionContextFeatureConfiguration>(featureConfiguration =>
                featureConfiguration.FeatureEnabled = true);

            return configuration;
        }
    }
}
