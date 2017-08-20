using EasyRpc.AspNetCore;
using EasyRpc.Tests.Classes;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using NSubstitute;
using SimpleFixture.xUnit;
using Xunit;

namespace EasyRpc.Tests.Middleware
{
    public class ExposeTypeSetTests : BaseRpcMiddlewareTests
    {
        [Theory]
        [AutoData]
        public void ExposeAssemblyContaining_Success(IApplicationBuilder app, HttpContext context)
        {
            Configure(app, "RpcApi", api =>
            {
                api.ExposeAssemblyContaining<ExposeTypeSetTests>()
                    .Where(TypesThat.AreInTheSameNamespaceAs(typeof(ComplexService)));
            });

            var result = MakeCall<ResultObject>(context, "/RpcApi/ComplexService", "Add",
                new {complex = new ComplexObject {A = 5, B = 10}});

            Assert.NotNull(result);
            Assert.Equal(15, result.Result);
        }

        [Theory]
        [AutoData]
        public void ExposeAssemblyContaining_Types_Success(IApplicationBuilder app, HttpContext context)
        {
            Configure(app, "RpcApi", api =>
            {
                api.ExposeAssemblyContaining<ExposeTypeSetTests>()
                    .Types(TypesThat.AreInTheSameNamespaceAs(typeof(ComplexService)));
            });

            var result = MakeCall<ResultObject>(context, "/RpcApi/ComplexService", "Add",
                new { complex = new ComplexObject { A = 5, B = 10 } });

            Assert.NotNull(result);
            Assert.Equal(15, result.Result);
        }

        [Theory]
        [AutoData]
        public void ExposeAssemblyContaining_Interfaces_Success(IApplicationBuilder app, HttpContext context)
        {
            context.RequestServices.GetService(typeof(IComplexService)).Returns(new ComplexService());

            Configure(app, "RpcApi", api =>
            {
                api.ExposeAssemblyContaining<ExposeTypeSetTests>()
                    .Interfaces(TypesThat.AreInTheSameNamespaceAs(typeof(ComplexService)));
            });

            var result = MakeCall<ResultObject>(context, "/RpcApi/IComplexService", "Add",
                new { complex = new ComplexObject { A = 5, B = 10 } });

            Assert.NotNull(result);
            Assert.Equal(15, result.Result);
        }
    }
}
