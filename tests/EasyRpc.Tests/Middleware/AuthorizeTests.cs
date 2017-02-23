using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using EasyRpc.AspNetCore.Messages;
using EasyRPC.AspNetCore.Tests.Classes;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using SimpleFixture.xUnit;
using Xunit;
using NSubstitute;

namespace EasyRpc.Tests.Middleware
{
    public class AuthorizeTests : BaseRpcMiddlewareTests
    {
        [Theory]
        [AutoData]
        public void Authorized_User_Success(IApplicationBuilder app, HttpContext context)
        {
            Configure(app, "RpcApi", api =>
            {
                api.Expose<IntMathService>().As("IntMath").Authorize();
            });

            context.User.Identity.IsAuthenticated.Returns(true);

            var value = MakeCall<int>(context, "/RpcApi/IntMath", "Add", new[] { 5, 10 });

            Assert.Equal(15, value);
        }

        [Theory]
        [AutoData]
        public void Authorized_User_Failure(IApplicationBuilder app, HttpContext context)
        {
            Configure(app, "RpcApi", api =>
            {
                api.Expose<IntMathService>().As("IntMath").Authorize();
            });

            context.User.Identity.IsAuthenticated.Returns(false);

            var value = MakeCall<ErrorResponseMessage>(context, "/RpcApi/IntMath", "Add", new[] { 5, 10 });

            Assert.Equal((int)JsonRpcErrorCode.UnauthorizedAccess, value.Error.Code);
            Assert.Equal((int)HttpStatusCode.Unauthorized, context.Response.StatusCode);
        }
    }
}
