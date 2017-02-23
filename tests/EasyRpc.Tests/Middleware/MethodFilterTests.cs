using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EasyRpc.AspNetCore.Messages;
using EasyRPC.AspNetCore.Tests.Classes;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using SimpleFixture.xUnit;
using Xunit;

namespace EasyRpc.Tests.Middleware
{
    public class MethodFilterTests : BaseRpcMiddlewareTests
    {

        [Theory]
        [AutoData]
        public void Filter_Success_Full_Call(IApplicationBuilder app, HttpContext context)
        {
            Configure(app, "RpcApi", api =>
            {
                api.MethodFilter(m => m.Name.StartsWith("Ad"));

                api.Expose<IntMathService>().As("IntMath");
            });

            var value = MakeCall<int>(context, "/RpcApi/IntMath", "Add", new[] {5, 10});

            Assert.Equal(15, value);

            var response = MakeCall<ErrorResponseMessage>(context, "/RpcApi/IntMath", "AsyncAdd", new[] { 5, 10 });

            Assert.Equal((int)JsonRpcErrorCode.MethodNotFound, response.Error.Code);
        }
    }
    }
