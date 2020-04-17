using System;
using System.Collections.Generic;
using System.Text;

namespace EasyRpc.Abstractions.Headers
{
    /// <summary>
    /// Attribute for setting cache control response header
    /// </summary>
    public class CacheControlAttribute : ResponseHeaderAttribute
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="value"></param>
        public CacheControlAttribute(string value) : base("Cache-Control", value)
        {
        }
    }
}
