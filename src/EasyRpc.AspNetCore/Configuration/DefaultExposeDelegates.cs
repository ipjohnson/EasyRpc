using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace EasyRpc.AspNetCore.Configuration
{
    public static class DefaultExposeDelegates
    {
        /// <summary>
        /// Default route name generator
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static string DefaultRouteNameGenerator(Type type)
        {
            return type.Name;
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

        public static bool DefaultTypeWrapSelector(Type wrapType)
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
