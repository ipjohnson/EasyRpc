using EasyRpc.Tests.Classes;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using SimpleFixture.NSubstitute;
using SimpleFixture.xUnit;
using Xunit;

namespace EasyRpc.Tests.Middleware
{

    [SubFixtureInitialize]
    public class JsonRpcMiddlewareTests : BaseRpcMiddlewareTests
    {
        [Theory]
        [AutoData]
        public void JsonRpcMiddleware_Success_Named_Parameter(IApplicationBuilder app, HttpContext context)
        {
            Configure(app, "RpcApi", api =>
            {
                api.Expose<IntMathService>().As("IntMath");
            });

            var result = MakeCall<int>(context, "/RpcApi/IntMath", "Add", new { a = 5, b = 10 });

            Assert.Equal(15, result);
        }

        [Theory]
        [AutoData]
        public void JsonRpcMiddleware_Success_Ordered_Parameter(IApplicationBuilder app, HttpContext context)
        {
            Configure(app, "RpcApi", api =>
            {
                api.Expose<IntMathService>().As("IntMath");
            });

            var result = MakeCall<int>(context, "/RpcApi/IntMath", "Add", new[] { 5, 10 });

            Assert.Equal(15, result);
        }

        [Theory]
        [AutoData]
        public void JsonRpcMiddleware_Success_Named_Parameter_Async(IApplicationBuilder app, HttpContext context)
        {
            Configure(app, "RpcApi", api =>
            {
                api.Expose<IntMathService>().As("IntMath");
            });

            var result = MakeCall<int>(context, "/RpcApi/IntMath", "AsyncAdd", new { a = 5, b = 10 });

            Assert.Equal(15, result);
        }

        [Theory]
        [AutoData]
        public void JsonRpcMiddleware_Success_Ordered_Parameter_Async(IApplicationBuilder app, HttpContext context)
        {
            Configure(app, "RpcApi", api =>
            {
                api.Expose<IntMathService>().As("IntMath");
            });

            var result = MakeCall<int>(context, "/RpcApi/IntMath", "AsyncAdd", new[] { 5, 10 });

            Assert.Equal(15, result);
        }
        
    }

}
