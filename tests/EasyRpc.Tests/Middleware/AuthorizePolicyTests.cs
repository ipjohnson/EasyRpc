using System.Net;
using System.Threading.Tasks;
using EasyRpc.AspNetCore.Messages;
using EasyRPC.AspNetCore.Tests.Classes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using SimpleFixture.xUnit;
using Xunit;
using NSubstitute;

namespace EasyRpc.Tests.Middleware
{
    public class AuthorizePolicyTests : BaseRpcMiddlewareTests
    {
        #region expose authorize
        [Theory]
        [AutoData]
        public void Authorize_UserPolicy_Success(IApplicationBuilder app, HttpContext context, IAuthorizationService authorizationService)
        {
            Configure(app, "RpcApi", api =>
            {
                api.Expose<IntMathService>().As("IntMath").Authorize(policy: "SomePolicy");
            });

            context.RequestServices.GetService(typeof(IAuthorizationService)).Returns(authorizationService);

            authorizationService.AuthorizeAsync(context.User, "SomePolicy").Returns(Task.FromResult(true));

            var value = MakeCall<int>(context, "/RpcApi/IntMath", "Add", new[] { 5, 10 });

            Assert.Equal(15, value);
        }

        [Theory]
        [AutoData]
        public void Authorize_UserPolicy_Failure(IApplicationBuilder app, HttpContext context, IAuthorizationService authorizationService)
        {
            Configure(app, "RpcApi", api =>
            {
                api.Expose<IntMathService>().As("IntMath").Authorize(policy: "SomePolicy");
            });

            context.RequestServices.GetService(typeof(IAuthorizationService)).Returns(authorizationService);

            authorizationService.AuthorizeAsync(context.User, "SomePolicy").Returns(Task.FromResult(false));

            var value = MakeCall<ErrorResponseMessage>(context, "/RpcApi/IntMath", "Add", new[] { 5, 10 });

            Assert.Equal((int)JsonRpcErrorCode.UnauthorizedAccess, value.Error.Code);
            Assert.Equal((int)HttpStatusCode.Unauthorized, context.Response.StatusCode);
        }
        #endregion

        #region api configure

        [Theory]
        [AutoData]
        public void Authorize_Api_UserPolicy_Success(IApplicationBuilder app, HttpContext context, IAuthorizationService authorizationService)
        {
            Configure(app, "RpcApi", api =>
            {
                api.Authorize(policy: "SomePolicy");
                api.Expose<IntMathService>().As("IntMath");
            });

            context.RequestServices.GetService(typeof(IAuthorizationService)).Returns(authorizationService);

            authorizationService.AuthorizeAsync(context.User, "SomePolicy").Returns(Task.FromResult(true));

            var value = MakeCall<int>(context, "/RpcApi/IntMath", "Add", new[] { 5, 10 });

            Assert.Equal(15, value);
        }

        [Theory]
        [AutoData]
        public void Authorize_Api_UserPolicy_Failure(IApplicationBuilder app, HttpContext context, IAuthorizationService authorizationService)
        {
            Configure(app, "RpcApi", api =>
            {
                api.Authorize(policy: "SomePolicy");
                api.Expose<IntMathService>().As("IntMath");
            });

            context.RequestServices.GetService(typeof(IAuthorizationService)).Returns(authorizationService);

            authorizationService.AuthorizeAsync(context.User, "SomePolicy").Returns(Task.FromResult(false));

            var value = MakeCall<ErrorResponseMessage>(context, "/RpcApi/IntMath", "Add", new[] { 5, 10 });

            Assert.Equal((int)JsonRpcErrorCode.UnauthorizedAccess, value.Error.Code);
            Assert.Equal((int)HttpStatusCode.Unauthorized, context.Response.StatusCode);
        }

        #endregion
    }
}
