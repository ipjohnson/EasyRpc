using System.ComponentModel.DataAnnotations;
using EasyRpc.AspNetCore.DataAnnotations;
using EasyRpc.AspNetCore.Messages;
using EasyRpc.Tests.Middleware;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using SimpleFixture.NSubstitute;
using SimpleFixture.xUnit;
using Xunit;

namespace EasyRpc.Tests.Validation
{
    public class TestClass
    {
        public string SomeMethod([Required]string value, [Required] string value2)
        {
            return value + value2;
        }
    }

    [SubFixtureInitialize]
    public class DataAnnotationValidationTests : BaseRpcMiddlewareTests
    {
        [Theory]
        [AutoData]
        public void FluentValidationSimple(IApplicationBuilder app, HttpContext context)
        {
            Configure(app, "RpcApi", api =>
            {
                api.UseDataAnnotations();
                api.Expose<TestClass>().As("SomeService");
            });

            var result = MakeCall<ErrorResponseMessage>(context, "/RpcApi/SomeService", "SomeMethod", new[] { "Value", null });

            Assert.NotNull(result);
            Assert.Equal((int)JsonRpcErrorCode.InvalidRequest, result.Error.Code);

            var stringResult = MakeCall<string>(context, "/RpcApi/SomeService", "SomeMethod", new[] { "Hello ", "World" });

            Assert.Equal("Hello World", stringResult);
        }
    }
}
