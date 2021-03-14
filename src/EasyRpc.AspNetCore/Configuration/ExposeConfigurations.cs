using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace EasyRpc.AspNetCore.Configuration
{
    public class ExposeConfigurations
    {
        /// <summary>
        /// Delegate to configure which return types should be wrapped in an object for serialization (string, int, double, DateTime etc)
        /// </summary>
        public TypeWrapSelector TypeWrapSelector { get; set; } = DefaultExposeDelegates.DefaultTypeWrapSelector;

        /// <summary>
        /// Delegate to pick which parameters types should be resolved from the container
        /// </summary>
        public Func<Type, bool> ResolveFromContainer { get; set; } = DefaultExposeDelegates.ResolveFromContainer;

        /// <summary>
        /// Single parameter methods with a body map the parameter without wrapper
        /// </summary>
        public bool SingleParameterPostFromBody { get; set; } = true;

        /// <summary>
        /// Route name generator for methods that haven't been attributed
        /// </summary>
        public Func<Type, string> RouteNameGenerator { get; set; } = DefaultExposeDelegates.DefaultRouteNameGenerator;

        /// <summary>
        /// Generate method name part of url for non attributed methods
        /// </summary>
        public Func<MethodInfo, string> MethodNameGenerator { get; set; } = DefaultExposeDelegates.DefaultMethodNameGenerator;

        /// <summary>
        /// Property type should be new data
        /// </summary>
        public Func<Type, bool> NewDataTypeFunc { get; set; } = t => false;
    }
}
