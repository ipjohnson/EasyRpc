using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace EasyRpc.AspNetCore.Configuration
{
    /// <summary>
    /// Default configuration delegates
    /// </summary>
    public static class DefaultExposeDelegates
    {
        private const string _moduleName = "Module";

        /// <summary>
        /// Default route name generator
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static string DefaultRouteNameGenerator(Type type)
        {
            var name = type.Name;

            if (name.EndsWith(_moduleName, StringComparison.CurrentCulture))
            {
                name = name.Substring(0, name.Length - _moduleName.Length);
            }

            return name;
        }

        /// <summary>
        /// Default method name generator
        /// </summary>
        /// <param name="method"></param>
        /// <returns></returns>
        public static string DefaultMethodNameGenerator(MethodInfo method)
        {
            return method.Name;
        }

        /// <summary>
        /// By default don't wrap anything
        /// </summary>
        /// <param name="wrapType"></param>
        /// <returns></returns>
        public static bool DefaultTypeWrapSelector(Type wrapType)
        {
            return false;
        }

        /// <summary>
        /// Wrap simple types with class
        /// </summary>
        /// <param name="wrapType"></param>
        /// <returns></returns>
        public static bool SimpleTypeWrapSelector(Type wrapType)
        {
            if (wrapType == typeof(bool) ||
                wrapType == typeof(byte) ||
                wrapType == typeof(byte[]) ||
                wrapType == typeof(int) ||
                wrapType == typeof(double) ||
                wrapType == typeof(decimal) ||
                wrapType == typeof(long) ||
                wrapType == typeof(float) ||
                wrapType == typeof(char) ||
                wrapType == typeof(string) ||
                wrapType == typeof(DateTime) ||
                wrapType == typeof(TimeSpan))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Resolve interface types from container
        /// </summary>
        /// <param name="arg"></param>
        /// <returns></returns>
        public static bool ResolveFromContainer(Type arg)
        {
            if (arg.IsInterface)
            {
                if (arg.IsConstructedGenericType)
                {
                    var openType = arg.GetGenericTypeDefinition();

                    if (openType == typeof(IEnumerable<>) ||
                        openType == typeof(ICollection<>) ||
                        openType == typeof(IList<>) ||
                        openType == typeof(IReadOnlyCollection<>) ||
                        openType == typeof(IReadOnlyList<>))
                    {
                        return ResolveFromContainer(arg.GenericTypeArguments[0]);
                    }
                }

                return true;
            }

            return false;
        }
    }
}
