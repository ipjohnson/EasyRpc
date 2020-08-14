using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace EasyRpc.DynamicClient
{
    /// <summary>
    /// Naming convention provider
    /// </summary>
    public interface INamingConventionService
    {
        /// <summary>
        /// Get name for type
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        string GetNameForType(Type type);

        /// <summary>
        /// Get method name
        /// </summary>
        /// <param name="method"></param>
        /// <returns></returns>
        string GetMethodName(MethodInfo method);
    }
}
