using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace EasyRpc.AspNetCore
{
    public interface IHeaderProcessor
    {
        void ProcessRequestHeaders(HttpContext context);

        void ProcessResponseHeaders(HttpContext context);
    }
}
