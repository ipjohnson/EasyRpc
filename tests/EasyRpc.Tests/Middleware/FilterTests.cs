using EasyRpc.AspNetCore;
using EasyRPC.AspNetCore.Tests.Classes;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using SimpleFixture.xUnit;
using Xunit;

namespace EasyRpc.Tests.Middleware
{
    public class FilterTests : BaseRpcMiddlewareTests
    {
        public class FilterClass : ICallExecuteFilter
        {
            public static int ExecuteCount = 0;

            public void AfterExecute(ICallExecutionContext context)
            {
                ExecuteCount++;
            }

            public void BeforeExecute(ICallExecutionContext context)
            {
                ExecuteCount++;
            }
        }

        [Theory]
        [AutoData]
        public void Filter_Success_Full_Call(IApplicationBuilder app, HttpContext context)
        {
            Configure(app, "RpcApi", api =>
            {
                api.ApplyFilter<FilterClass>();
                api.Expose<IntMathService>().As("IntMath");
            });

            FilterClass.ExecuteCount = 0;

            var value = MakeCall<int>(context, "/RpcApi/IntMath", "Add", new[] { 5, 10 });

            Assert.Equal(15, value);
            Assert.Equal(2, FilterClass.ExecuteCount);
        }
    }
}
