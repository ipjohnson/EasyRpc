using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.AspNetCore.Http;

namespace EasyRpc.AspNetCore.Utilities
{
    public static class ContextExtentions
    {
        public static bool SupportsGzipCompression(this HttpContext context)
        {
            var header = context.Request.Headers["Accept-Encoding"];

            return header.Any(s => s.Contains("gzip"));
        }
    }
}
