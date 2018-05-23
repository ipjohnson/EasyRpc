using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using EasyRpc.AspNetCore.Middleware;
using Microsoft.AspNetCore.Http;

namespace EasyRpc.AspNetCore.Documentation
{
    public class DocumentationRequestProcessor
    {
        public void Configure(EndPointConfiguration configuration)
        {

        }

        public Task ProcessRequest(HttpContext context)
        {
            return Task.CompletedTask;
        }
    }
}
