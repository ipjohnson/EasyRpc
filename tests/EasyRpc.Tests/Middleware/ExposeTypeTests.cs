using EasyRpc.AspNetCore.Messages;
using EasyRpc.Tests.Classes;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using SimpleFixture.xUnit;
using Xunit;

namespace EasyRpc.Tests.Middleware
{
    public class ExposeTypeTests : BaseRpcMiddlewareTests
    {
        [Theory]
        [AutoData]
        public void ExposeType(IApplicationBuilder app, HttpContext context)
        {
            Configure(app, "RpcApi", api =>
            {
                api.Expose(typeof(IntMathService)).As("IntMath");
            });
            
            var value = MakeCall<int>(context, "/RpcApi/IntMath", "Add", new[] { 5, 10 });

            Assert.Equal(15, value);
        }
        
        [Theory]
        [AutoData]
        public void ExposeTypeGeneric(IApplicationBuilder app, HttpContext context)
        {
            Configure(app, "RpcApi", api =>
            {
                api.Expose<IntMathService>().As("IntMath");
            });

            var value = MakeCall<int>(context, "/RpcApi/IntMath", "Add", new[] { 5, 10 });

            Assert.Equal(15, value);
        }

        public class MultipleMethodClass
        {
            public int Add(int a, int b)
            {
                return a + b;
            }

            public int Subtract(int a, int b)
            {
                return a - b;
            }

            public int Test(ComplexObject model)
            {
                return model.A + model.B;
            }
        }

        [Theory]
        [AutoData]
        public void ExposeTypeFilterMethods(IApplicationBuilder app, HttpContext context)
        {
            Configure(app, "RpcApi", api =>
            {
                api.Expose(typeof(MultipleMethodClass)).As("IntMath");
            });
            
            var value = MakeCall<int>(context, "/RpcApi/IntMath", "Test", new[] { new ComplexObject{ A = 5, B = 10} });

            Assert.Equal(15, value);
        }

        [Theory]
        [AutoData]
        public void ExposeTypeGenericFilterMethods(IApplicationBuilder app, HttpContext context)
        {
            Configure(app, "RpcApi", api =>
            {
                api.Expose<MultipleMethodClass>().As("IntMath").Methods(m => m.Name == "Add");
            });

            var errorValue = MakeCall<ErrorResponseMessage>(context, "/RpcApi/IntMath", nameof(MultipleMethodClass.Subtract), new[] { 5, 10 });

            Assert.Equal((int)JsonRpcErrorCode.MethodNotFound, errorValue.Error.Code);

            var value = MakeCall<int>(context, "/RpcApi/IntMath", "Add", new[] { 5, 10 });

            Assert.Equal(15, value);
        }
    }
}
