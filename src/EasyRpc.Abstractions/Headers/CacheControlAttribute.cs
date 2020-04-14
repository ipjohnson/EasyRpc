using System;
using System.Collections.Generic;
using System.Text;

namespace EasyRpc.Abstractions.Headers
{
    public class CacheControlAttribute : ResponseHeaderAttribute
    {
        public CacheControlAttribute(string value) : base("Cache-Control", value)
        {
        }
    }
}
