using System.Net;
using EasyRpc.AspNetCore.Messages;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using SimpleFixture.xUnit;
using Xunit;
using NSubstitute;
using EasyRpc.Tests.Classes;

namespace EasyRpc.Tests.Middleware
{
    public class AuthorizeRoleTests : BaseRpcMiddlewareTests
    {
        #region expose authorize
        [Theory]
        [AutoData]
        public void Authorize_UserRole_Success(IApplicationBuilder app, HttpContext context)
        {
            Configure(app, "RpcApi", api =>
            {
                api.Expose<IntMathService>().As("IntMath").Authorize(role: "SomeRole");
            });

            context.User.IsInRole("SomeRole").Returns(true);

            var value = MakeCall<int>(context, "/RpcApi/IntMath", "Add", new[] { 5, 10 });

            Assert.Equal(15, value);
        }

        [Theory]
        [AutoData]
        public void Authorize_UserRole_Failure(IApplicationBuilder app, HttpContext context)
        {
            Configure(app, "RpcApi", api =>
            {
                api.Expose<IntMathService>().As("IntMath").Authorize(role: "SomeRole");
            });

            context.User.IsInRole("SomeRole").Returns(false);

            var value = MakeCall<ErrorResponseMessage>(context, "/RpcApi/IntMath", "Add", new[] { 5, 10 });

            Assert.Equal((int)JsonRpcErrorCode.UnauthorizedAccess, value.Error.Code);
            Assert.Equal((int)HttpStatusCode.Unauthorized, context.Response.StatusCode);
        }
        #endregion

        #region api configure

        [Theory]
        [AutoData]
        public void Authorize_Api_UserRole_Success(IApplicationBuilder app, HttpContext context)
        {
            Configure(app, "RpcApi", api =>
            {
                api.Authorize(role: "SomeRole");
                api.Expose<IntMathService>().As("IntMath");
            });

            context.User.IsInRole("SomeRole").Returns(true);

            var value = MakeCall<int>(context, "/RpcApi/IntMath", "Add", new[] { 5, 10 });

            Assert.Equal(15, value);
        }

        [Theory]
        [AutoData]
        public void Authorize_Api_UserRole_Failure(IApplicationBuilder app, HttpContext context)
        {
            Configure(app, "RpcApi", api =>
            {
                api.Authorize(role: "SomeRole");
                api.Expose<IntMathService>().As("IntMath");
            });

            context.User.IsInRole("SomeRole").Returns(false);

            var value = MakeCall<ErrorResponseMessage>(context, "/RpcApi/IntMath", "Add", new[] { 5, 10 });

            Assert.Equal((int)JsonRpcErrorCode.UnauthorizedAccess, value.Error.Code);
            Assert.Equal((int)HttpStatusCode.Unauthorized, context.Response.StatusCode);
        }


        #endregion
    }
}
