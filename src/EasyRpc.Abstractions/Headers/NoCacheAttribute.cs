using System;
using System.Collections.Generic;
using System.Text;

namespace EasyRpc.Abstractions.Headers
{
    public class NoCacheAttribute : CacheControlAttribute
    {
        public NoCacheAttribute() : base("no-cache")
        {
        }
    }
}
