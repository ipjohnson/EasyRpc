using EasyRpc.Tests.Classes;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using SimpleFixture.xUnit;
using Xunit;

namespace EasyRpc.Tests.Middleware
{
    public class DefaultValueTests : BaseRpcMiddlewareTests
    {
        [Theory]
        [AutoData]
        public void DefaultValue_NamedParameter(IApplicationBuilder app, HttpContext context)
        {
            Configure(app, "RpcApi", api =>
            {
                api.Expose<DefaultValueService>().As("DefaultValue");
            });

            var result = MakeCall<string>(context, "/RpcApi/DefaultValue", "SomeMethod", new { baseString = "SomeString" });

            Assert.Equal("SomeString" + DefaultValueService.DefaultValueString, result);

            result = MakeCall<string>(context, "/RpcApi/DefaultValue", "SomeMethod", new { baseString = "SomeString", endString="-OtherValue" });

            Assert.Equal("SomeString-OtherValue", result);
        }

        [Theory]
        [AutoData]
        public void DefaultValue_OrderedParameter(IApplicationBuilder app, HttpContext context)
        {
            Configure(app, "RpcApi", api =>
            {
                api.Expose<DefaultValueService>().As("DefaultValue");
            });

            var result = MakeCall<string>(context, "/RpcApi/DefaultValue", "SomeMethod", new [] { "SomeString" });

            Assert.Equal("SomeString" + DefaultValueService.DefaultValueString, result);

            result = MakeCall<string>(context, "/RpcApi/DefaultValue", "SomeMethod", new [] { "SomeString", "-OtherValue" });

            Assert.Equal("SomeString-OtherValue", result);
        }
    }
}
