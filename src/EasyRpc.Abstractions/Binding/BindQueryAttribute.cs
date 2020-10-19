using System;
using System.Collections.Generic;
using System.Text;

namespace EasyRpc.Abstractions.Binding
{
    /// <summary>
    /// 
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter)]
    public class BindQueryAttribute : Attribute
    {
        /// <summary>
        /// Query string
        /// </summary>
        public string Name { get; set; }
    }
}
