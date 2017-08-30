using System.Collections.Generic;
using EasyRpc.AspNetCore.FluentValidation;
using EasyRpc.AspNetCore.Messages;
using EasyRpc.Tests.Middleware;
using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using NSubstitute;
using SimpleFixture.NSubstitute;
using SimpleFixture.xUnit;
using Xunit;

namespace EasyRpc.Tests.Validation
{
    public class DataModel
    {
        public string SomeString { get; set; }
    }

    public class DataModelValidation : AbstractValidator<DataModel>
    {
        public DataModelValidation()
        {
            RuleFor(a => a.SomeString).NotEmpty();
        }
    }

    public class SomeImplementation
    {
        public string Replace(DataModel model)
        {
            return model.SomeString.Replace("Hello", "Goodbye");
        }
    }

    [SubFixtureInitialize]
    public class FluentValidationTests : BaseRpcMiddlewareTests
    {
        [Theory]
        [AutoData]
        public void FluentValidationSimple(IApplicationBuilder app, HttpContext context)
        {
            app.ApplicationServices.GetService(typeof(IEnumerable<IValidator<DataModel>>))
                .Returns(new IValidator<DataModel>[] { new DataModelValidation() });

            Configure(app, "RpcApi", api =>
            {
                api.UseFluentValidation();
                api.Expose<SomeImplementation>().As("SomeService");
            });

            var result = MakeCall<ErrorResponseMessage>(context, "/RpcApi/SomeService", "Replace", new[] { new DataModel { SomeString = null } });

            Assert.NotNull(result);
            Assert.Equal((int)JsonRpcErrorCode.InvalidRequest, result.Error.Code);

            var stringResult = MakeCall<string>(context, "/RpcApi/SomeService", "Replace", new[] { new DataModel { SomeString = "Hello World" } });

            Assert.Equal("Goodbye World", stringResult);
        }
    }
}
