using System;
using System.Collections.Generic;
using System.Text;

namespace EasyRpc.Abstractions.Binding
{
    /// <summary>
    /// Bind from the route 
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter)]
    public class BindRouteAttribute : Attribute
    {
        /// <summary>
        /// Route name if different from property
        /// </summary>
        public string Name { get; set; }
    }
}
