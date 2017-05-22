using System;
using System.Collections.Generic;
using System.Reflection;

namespace EasyRpc.AspNetCore.Middleware
{
    /// <summary>
    /// Naming convention
    /// </summary>
    public class NamingConventions
    {
        private Func<Type, IEnumerable<string>> _routeNameGenerator = DefaultRouteNameGenerator;
        private Func<MethodInfo, string> _methodNameGenerator = DefaultMethodNameGenerator;

        /// <summary>
        /// Function used to create route names for types, by default it's just the type name
        /// </summary>
        public Func<Type, IEnumerable<string>> RouteNameGenerator
        {
            get { return _routeNameGenerator; }
            set
            {
                if(value == null) throw new ArgumentNullException(nameof(RouteNameGenerator));

                _routeNameGenerator = value;
            }
        }

        /// <summary>
        /// Function used to generate method names, by default it's just the method name
        /// </summary>
        public Func<MethodInfo, string> MethodNameGenerator
        {
            get { return _methodNameGenerator; }
            set
            {
                if(value == null) throw new ArgumentNullException(nameof(MethodNameGenerator));

                _methodNameGenerator = value;
            }
        }

        /// <summary>
        /// Default route name generator
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static IEnumerable<string> DefaultRouteNameGenerator(Type type)
        {
            yield return type.Name;
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
    }
}
