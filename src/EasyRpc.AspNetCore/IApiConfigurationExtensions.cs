using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace EasyRpc.AspNetCore
{
    /// <summary>
    /// static extension class for IApiConfiguration
    /// </summary>
    public static class IApiConfigurationExtensions
    {
        /// <summary>
        /// Expose types in the same assembly and namespace as T
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="configuration"></param>
        /// <returns></returns>
        public static  ITypeSetExposureConfiguration ExposeNamespaceContaining<T>(this IApiConfiguration configuration)
        {
            if (configuration == null) throw new ArgumentNullException(nameof(configuration));

            return configuration.Expose(typeof(T).GetTypeInfo().Assembly.ExportedTypes.Where(TypesThat.AreInTheSameNamespaceAs<T>()));
        }
    }
}
