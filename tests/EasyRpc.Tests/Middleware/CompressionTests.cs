using System;
using System.Collections.Generic;
using System.Text;
using EasyRpc.AspNetCore;
using EasyRPC.AspNetCore.Tests.Classes;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using SimpleFixture.xUnit;
using Xunit;

namespace EasyRpc.Tests.Middleware
{
    public class CompressionTests : BaseRpcMiddlewareTests
    {
        [Theory]
        [AutoData]
        public void ResponseCompression(IApplicationBuilder app, HttpContext context)
        {
            Configure(app, "RpcApi", api =>
            {
                api.Expose<IntMathService>().As("IntMath");
            }, 
            new RpcServiceConfiguration { SupportResponseCompression = true });

            context.Request.Headers["Accept-Encoding"] = new StringValues("gzip");

            var result = MakeCall<int>(context, "/RpcApi/IntMath", "AsyncAdd", new[] { 5, 10 });

            Assert.Equal(15, result);
        }
        
        [Theory]
        [AutoData]
        public void RequestCompression(IApplicationBuilder app, HttpContext context)
        {
            Configure(app, "RpcApi", api =>
                {
                    api.Expose<IntMathService>().As("IntMath");
                },
                new RpcServiceConfiguration { SupportRequestCompression = true });
            
            var result = MakeCall<int>(context, "/RpcApi/IntMath", "AsyncAdd", new[] { 5, 10 }, compress: true);

            Assert.Equal(15, result);
        }
    }
}
