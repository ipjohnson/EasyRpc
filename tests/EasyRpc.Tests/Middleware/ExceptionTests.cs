using System;
using EasyRpc.AspNetCore.Messages;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using SimpleFixture.xUnit;
using Xunit;

namespace EasyRpc.Tests.Middleware
{
    public class ExceptionTests : BaseRpcMiddlewareTests
    {
        #region Throws on Create
        public class ThrowsOnCreate
        {
            public const string ExceptionMessage = "Throw On Create";

            public ThrowsOnCreate()
            {
                throw new Exception(ExceptionMessage);
            }

            public void Test()
            {

            }
        }


        [Theory]
        [AutoData]
        public void Exception_Throw_Create_Instance_Show_Message(IApplicationBuilder app, HttpContext context)
        {
            Configure(app, "RpcApi", api =>
            {
                api.Expose<ThrowsOnCreate>();
            });

            var errorResponse = MakeCall<ErrorResponseMessage>(context, "/RpcApi/ThrowsOnCreate", "Test", new { });

            Assert.NotNull(errorResponse);
            Assert.NotNull(errorResponse.Error);
            Assert.Equal((int)JsonRpcErrorCode.InternalServerError, errorResponse.Error.Code);
            Assert.Contains(ThrowsOnCreate.ExceptionMessage, errorResponse.Error.Message);
        }
        #endregion

        #region Throws On Execute

        public class ThrowsOnExecute
        {
            public const string ExceptionMessage = "Throws On Execute";

            public int Add(int a, int b)
            {
                throw new Exception(ExceptionMessage);
            }
        }

        [Theory]
        [AutoData]
        public void Exception_Throw_Execute_Show_Message(IApplicationBuilder app, HttpContext context)
        {
            Configure(app, "RpcApi", api =>
            {
                api.Expose<ThrowsOnExecute>();
            });

            var errorResponse = MakeCall<ErrorResponseMessage>(context, "/RpcApi/ThrowsOnExecute", "Add", new { a = 1, b = 2 });

            Assert.NotNull(errorResponse);
            Assert.NotNull(errorResponse.Error);
            Assert.Equal((int)JsonRpcErrorCode.InternalServerError, errorResponse.Error.Code);
            Assert.Contains(ThrowsOnExecute.ExceptionMessage, errorResponse.Error.Message);
        }

        #endregion
    }
}
