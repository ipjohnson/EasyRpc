using System.Collections.Generic;
using EasyRpc.AspNetCore;
using EasyRpc.Tests.Classes;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using SimpleFixture.xUnit;
using Xunit;

namespace EasyRpc.Tests.Middleware
{
    public abstract class CompressionTests : BaseRpcMiddlewareTests
    {
        protected abstract string Compression { get; }

        [Theory]
        [AutoData]
        public void SkipCompressionIntReturn(IApplicationBuilder app, HttpContext context)
        {
            Configure(app, "RpcApi", api =>
            {
                api.Expose<IntMathService>().As("IntMath");
            },
            new RpcServiceConfiguration { SupportResponseCompression = true });

            context.Request.Headers["Accept-Encoding"] = new StringValues(Compression);

            var result = MakeCall<int>(context, "/RpcApi/IntMath", "Add", new[] { 5, 10 });

            Assert.Equal(15, result);
            var encoding = context.Response.Headers["Content-Encoding"];

            Assert.Empty(encoding);
        }

        [Theory]
        [AutoData]
        public void SkipCompressionTaskIntReturn(IApplicationBuilder app, HttpContext context)
        {
            Configure(app, "RpcApi", api =>
            {
                api.Expose<IntMathService>().As("IntMath");
            },
            new RpcServiceConfiguration { SupportResponseCompression = true });

            context.Request.Headers["Accept-Encoding"] = new StringValues(Compression);

            var result = MakeCall<int>(context, "/RpcApi/IntMath", "AsyncAdd", new[] { 5, 10 });

            Assert.Equal(15, result);
            var encoding = context.Response.Headers["Content-Encoding"];

            Assert.Empty(encoding);
        }

        [Theory]
        [AutoData]
        public void ResponseCompressionComplexReturn(IApplicationBuilder app, HttpContext context)
        {
            Configure(app, "RpcApi", api =>
            {
                api.Expose<ComplexService>().As("Complex");
            },
                new RpcServiceConfiguration { SupportResponseCompression = true });

            var result = MakeCall<ResultObject>(context, "/RpcApi/Complex", "Add", new[] { new ComplexObject { A = 5, B = 10 } }, compressResponse: Compression);

            Assert.Equal(15, result.Result);

            var encoding = context.Response.Headers["Content-Encoding"];

            Assert.Single(encoding);
            Assert.Equal(Compression, encoding[0]);
        }


        [Theory]
        [AutoData]
        public void ResponseCompressionComplexNullReturn(IApplicationBuilder app, HttpContext context)
        {
            Configure(app, "RpcApi", api =>
            {
                api.Expose<ComplexService>().As("Complex");
            },
            new RpcServiceConfiguration { SupportResponseCompression = true });

            var result = MakeCall<ResultObject>(context, "/RpcApi/Complex", "ReturnNull", new[] { new ComplexObject { A = 5, B = 10 } }, compressResponse: Compression);

            Assert.Null(result);

            var encoding = context.Response.Headers["Content-Encoding"];

            Assert.Empty(encoding);
        }

        #region List tests

        [Theory]
        [AutoData]
        public void ResponseCompressionListReturn(IApplicationBuilder app, HttpContext context)
        {
            Configure(app, "RpcApi", api =>
            {
                api.Expose<ComplexService>().As("Complex");
            },
            new RpcServiceConfiguration { SupportResponseCompression = true });

            var callList = new List<ComplexObject> { new ComplexObject { A = 5, B = 10 } };

            var result = MakeCall<List<ResultObject>>(context, "/RpcApi/Complex", "AddList", new[] { callList }, compressResponse: Compression);

            Assert.Single(result);
            Assert.Equal(15, result[0].Result);

            var encoding = context.Response.Headers["Content-Encoding"];

            Assert.Single(encoding);
            Assert.Equal(Compression, encoding[0]);
        }

        [Theory]
        [AutoData]
        public void SkipCompressionEmptyListReturn(IApplicationBuilder app, HttpContext context)
        {
            Configure(app, "RpcApi", api =>
            {
                api.Expose<ComplexService>().As("Complex");
            },
                new RpcServiceConfiguration { SupportResponseCompression = true });

            context.Request.Headers["Accept-Encoding"] = new StringValues(Compression);

            var callList = new List<ComplexObject>();

            var result = MakeCall<List<ResultObject>>(context, "/RpcApi/Complex", "AddList", new[] { callList });

            Assert.Empty(result);

            var encoding = context.Response.Headers["Content-Encoding"];

            Assert.Empty(encoding);
        }

        [Theory]
        [AutoData]
        public void ResponseAsyncCompressionListReturn(IApplicationBuilder app, HttpContext context)
        {
            Configure(app, "RpcApi", api =>
            {
                api.Expose<ComplexService>().As("Complex");
            },
                new RpcServiceConfiguration { SupportResponseCompression = true });

            var callList = new List<ComplexObject> { new ComplexObject { A = 5, B = 10 } };

            var result = MakeCall<List<ResultObject>>(context, "/RpcApi/Complex", "AsyncAddList", new[] { callList }, compressResponse: Compression);

            Assert.Single(result);
            Assert.Equal(15, result[0].Result);

            var encoding = context.Response.Headers["Content-Encoding"];

            Assert.Single(encoding);
            Assert.Equal(Compression, encoding[0]);
        }

        [Theory]
        [AutoData]
        public void SkipAsyncCompressionEmptyListReturn(IApplicationBuilder app, HttpContext context)
        {
            Configure(app, "RpcApi", api =>
            {
                api.Expose<ComplexService>().As("Complex");
            },
                new RpcServiceConfiguration { SupportResponseCompression = true });

            context.Request.Headers["Accept-Encoding"] = new StringValues(Compression);

            var callList = new List<ComplexObject>();

            var result = MakeCall<List<ResultObject>>(context, "/RpcApi/Complex", "AsyncAddList", new[] { callList });

            Assert.Empty(result);

            var encoding = context.Response.Headers["Content-Encoding"];

            Assert.Empty(encoding);
        }

        #endregion

        #region String Tests

        [Theory]
        [AutoData]
        public void ResponseCompressionBigStringReturn(IApplicationBuilder app, HttpContext context)
        {
            Configure(app, "RpcApi", api =>
            {
                api.Expose<ComplexService>().As("Complex");
            },
                new RpcServiceConfiguration { SupportResponseCompression = true });

            var result = MakeCall<string>(context, "/RpcApi/Complex", "ReturnString", new[] { 1000 }, compressResponse: Compression);

            Assert.Equal(1000, result.Length);

            var encoding = context.Response.Headers["Content-Encoding"];

            Assert.Single(encoding);
            Assert.Equal(Compression, encoding[0]);
        }

        [Theory]
        [AutoData]
        public void SkipCompressionSmallStringReturn(IApplicationBuilder app, HttpContext context)
        {
            Configure(app, "RpcApi", api =>
            {
                api.Expose<ComplexService>().As("Complex");
            },
                new RpcServiceConfiguration { SupportResponseCompression = true });

            var result = MakeCall<string>(context, "/RpcApi/Complex", "ReturnString", new[] { 500 }, compressResponse: Compression);

            Assert.Equal(500, result.Length);

            var encoding = context.Response.Headers["Content-Encoding"];

            Assert.Empty(encoding);
        }

        [Theory]
        [AutoData]
        public void ResponseAsyncCompressionBigStringReturn(IApplicationBuilder app, HttpContext context)
        {
            Configure(app, "RpcApi", api =>
            {
                api.Expose<ComplexService>().As("Complex");
            }, new RpcServiceConfiguration { SupportResponseCompression = true });

            var result = MakeCall<string>(context, "/RpcApi/Complex", "AsyncReturnString", new[] { 1000 }, compressResponse: Compression);

            Assert.Equal(1000, result.Length);

            var encoding = context.Response.Headers["Content-Encoding"];

            Assert.Single(encoding);
            Assert.Equal(Compression, encoding[0]);
        }

        [Theory]
        [AutoData]
        public void SkipAsyncCompressionSmallStringReturn(IApplicationBuilder app, HttpContext context)
        {
            Configure(app, "RpcApi", api =>
            {
                api.Expose<ComplexService>().As("Complex");
            },
                new RpcServiceConfiguration { SupportResponseCompression = true });

            var result = MakeCall<string>(context, "/RpcApi/Complex", "AsyncReturnString", new[] { 500 }, compressResponse: Compression);

            Assert.Equal(500, result.Length);

            var encoding = context.Response.Headers["Content-Encoding"];

            Assert.Empty(encoding);
        }


        #endregion

        #region ResponseMessage

        [Theory]
        [AutoData]
        public void ResponseCompressionResponseMessageReturn(IApplicationBuilder app, HttpContext context)
        {
            Configure(app, "RpcApi", api =>
            {
                api.Expose<ComplexService>().As("Complex");
            },
                new RpcServiceConfiguration { SupportResponseCompression = true });

            var result = MakeCall<string>(context, "/RpcApi/Complex", "ReturnResponseMessage", new[] { true }, compressResponse: Compression);

            Assert.Equal(1000, result.Length);

            var encoding = context.Response.Headers["Content-Encoding"];

            Assert.Single(encoding);
            Assert.Equal(Compression, encoding[0]);
        }

        [Theory]
        [AutoData]
        public void SkipCompressionResponseMessageReturn(IApplicationBuilder app, HttpContext context)
        {
            Configure(app, "RpcApi", api =>
            {
                api.Expose<ComplexService>().As("Complex");
            },
                new RpcServiceConfiguration { SupportResponseCompression = true });

            context.Request.Headers["Accept-Encoding"] = new StringValues(Compression);

            var result = MakeCall<string>(context, "/RpcApi/Complex", "ReturnResponseMessage", new[] { false });

            Assert.Equal(1000, result.Length);

            var encoding = context.Response.Headers["Content-Encoding"];

            Assert.Empty(encoding);
        }

        [Theory]
        [AutoData]
        public void ResponseCompressionAsyncResponseMessageReturn(IApplicationBuilder app, HttpContext context)
        {
            Configure(app, "RpcApi", api =>
            {
                api.Expose<ComplexService>().As("Complex");
            },
                new RpcServiceConfiguration { SupportResponseCompression = true });

            var result = MakeCall<string>(context, "/RpcApi/Complex", "AsyncReturnResponseMessage", new[] { true }, compressResponse: Compression);

            Assert.Equal(1000, result.Length);

            var encoding = context.Response.Headers["Content-Encoding"];

            Assert.Single(encoding);
            Assert.Equal(Compression, encoding[0]);
        }
        
        [Theory]
        [AutoData]
        public void SkipCompressionAsyncResponseMessageReturn(IApplicationBuilder app, HttpContext context)
        {
            Configure(app, "RpcApi", api =>
            {
                api.Expose<ComplexService>().As("Complex");
            },
                new RpcServiceConfiguration { SupportResponseCompression = true });

            context.Request.Headers["Accept-Encoding"] = new StringValues(Compression);

            var result = MakeCall<string>(context, "/RpcApi/Complex", "AsyncReturnResponseMessage", new[] { false });

            Assert.Equal(1000, result.Length);

            var encoding = context.Response.Headers["Content-Encoding"];

            Assert.Empty(encoding);
        }
        #endregion

        #region request

        [Theory]
        [AutoData]
        public void RequestCompression(IApplicationBuilder app, HttpContext context)
        {
            Configure(app, "RpcApi", api =>
            {
                api.Expose<IntMathService>().As("IntMath");
            },
                new RpcServiceConfiguration { SupportRequestCompression = true });

            var result = MakeCall<int>(context, "/RpcApi/IntMath", "AsyncAdd", new[] { 5, 10 }, compressRequest: Compression);

            Assert.Equal(15, result);
        }

        #endregion

    }
}
