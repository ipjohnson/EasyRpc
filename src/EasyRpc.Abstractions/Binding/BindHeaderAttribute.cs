using System;
using System.Collections.Generic;
using System.Text;

namespace EasyRpc.Abstractions.Binding
{
    /// <summary>
    /// Bind from header
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter)]
    public class BindHeaderAttribute : Attribute
    {
        /// <summary>
        /// Header name if different from property
        /// </summary>
        public string Name { get; set; }
    }
}
