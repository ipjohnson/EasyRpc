using EasyRpc.Tests.Classes;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using SimpleFixture.xUnit;
using Xunit;

namespace EasyRpc.Tests.Middleware
{
    public class PrefixTests : BaseRpcMiddlewareTests
    {
        [Theory]
        [AutoData]
        public void Prefix_Multiple(IApplicationBuilder app, HttpContext context)
        {
            Configure(app, "RpcApi", api =>
            {
                api.Prefix("v1/");
                api.Prefix("v2/");
                api.Expose<IntMathService>().As("IntMath");
            });

            var value = MakeCall<int>(context, "/RpcApi/v1/IntMath", "Add", new[] { 5, 10 });

            Assert.Equal(15, value);

            value = MakeCall<int>(context, "/RpcApi/v2/IntMath", "Add", new[] { 5, 10 });

            Assert.Equal(15, value);
        }

        [Theory]
        [AutoData]
        public void Prefix_Multiple_Clear(IApplicationBuilder app, HttpContext context)
        {
            Configure(app, "RpcApi", api =>
            {
                api.Prefix("v1/");
                api.Expose<IntMathService>().As("IntMath");

                api.ClearPrefixes();
                api.Expose<DefaultValueService>().As("DefaultValue");
            });

            var value = MakeCall<int>(context, "/RpcApi/v1/IntMath", "Add", new[] { 5, 10 });

            Assert.Equal(15, value);

            var stringValue = MakeCall<string>(context, "/RpcApi/DefaultValue", "SomeMethod", new[] { "Start", "-End" });

            Assert.Equal("Start-End", stringValue);
        }
    }
}
