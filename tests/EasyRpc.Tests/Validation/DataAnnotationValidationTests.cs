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
        public void DataAnnotationValidationSimple(IApplicationBuilder app, HttpContext context)
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

        public class StringValues
        {
            [Required]
            public string A { get; set; }

            [Required]
            public string B { get; set; }
        }

        public class StringValuesService
        {
            public string Execute(StringValues values)
            {
                return values.A + " " + values.B;
            }
        }


        [Theory]
        [AutoData]
        public void DataAnnotationObjectValidation(IApplicationBuilder app, HttpContext context)
        {
            Configure(app, "RpcApi", api =>
            {
                api.UseDataAnnotations();
                api.Expose<StringValuesService>().As("SomeService");
            });

            var result = MakeCall<ErrorResponseMessage>(context, "/RpcApi/SomeService", "Execute", new [] { new StringValues() });

            Assert.NotNull(result);
            Assert.Equal((int)JsonRpcErrorCode.InvalidRequest, result.Error.Code);

            var stringResult = MakeCall<string>(context, "/RpcApi/SomeService", "Execute", new[] { new StringValues{ A = "Hello", B = "World" } });

            Assert.Equal("Hello World", stringResult);
        }


        public interface IMultipleValidationClass
        {
            string Execute([Required] string string1,
                [Required] [StringLength(25, MinimumLength = 10)] string string2);
        }

        public class MultipleValidationClass : IMultipleValidationClass
        {
            public string Execute([Required] string string1,
                [Required] [StringLength(25, MinimumLength = 10)] string string2)
            {
                return string1 + " " + string2;
            }
        }


        // commenting out till ByInterfaces is implemented
        //[Theory]
        //[AutoData]
        //public void DataAnnotationParameterValidation(IApplicationBuilder app, HttpContext context)
        //{
        //    Configure(app, "RpcApi", api =>
        //    {
        //        api.UseDataAnnotations();
        //        api.Expose(new[] {typeof(MultipleValidationClass)}).Interfaces();
        //    });

        //    var result = MakeCall<ErrorResponseMessage>(context, "/RpcApi/IMultipleValidationClass", "Execute", new[] { "Some", "string" });

        //    Assert.NotNull(result);
        //    Assert.Equal((int)JsonRpcErrorCode.InvalidRequest, result.Error.Code);

        //    var stringResult = MakeCall<string>(context, "/RpcApi/IMultipleValidationClass", "Execute", new[] {  "Hello", "Long World!"  });

        //    Assert.Equal("Hello Long World!", stringResult);
        //}
    }
}
