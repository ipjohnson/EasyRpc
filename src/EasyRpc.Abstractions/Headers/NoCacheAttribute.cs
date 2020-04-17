using System;
using System.Collections.Generic;
using System.Text;

namespace EasyRpc.Abstractions.Headers
{
    /// <summary>
    /// Sets the cache control header to be no-cache
    /// </summary>
    public class NoCacheAttribute : CacheControlAttribute
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public NoCacheAttribute() : base("no-cache")
        {
        }
    }
}
