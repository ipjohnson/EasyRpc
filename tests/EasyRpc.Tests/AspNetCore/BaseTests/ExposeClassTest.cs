using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using EasyRpc.AspNetCore;
using EasyRpc.Tests.Services.Models;
using EasyRpc.Tests.Services.SimpleServices;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Xunit;


namespace EasyRpc.Tests.AspNetCore.BaseTests
{
    public class ExposeClassTest : BaseRequestTest
    {
        [Fact]
        public async Task ExposeString()
        {
            var response = await Get("/stringtest/Yeah");

            var result = await Deserialize<string>(response);

            Assert.NotNull(result);
        }

        [Fact]
        public async Task ExposeAttributePost()
        {
            var response = await Post("/AddGenericResult", new { a = 2, b = 2 });

            var result = await Deserialize<GenericResult<int>>(response);

            Assert.NotNull(result);
            Assert.Equal(4, result.Result);
        }

        [Fact]
        public async Task ExposeAttributeGet()
        {
            var response = await Get("/RandomNumber");

            var result = await Deserialize<GenericResult<int>>(response);

            Assert.NotNull(result);
            Assert.NotEqual(0, result.Result);
        }

        [Fact]
        public async Task ExposeAttributeGetAsync()
        {
            var response = await Get("/RandomNumberAsync");

            var result = await Deserialize<GenericResult<int>>(response);

            Assert.NotNull(result);
            Assert.NotEqual(0, result.Result);
        }


        protected override void ApiRegistration(IRpcApi api)
        {
            api.Expose<AttributedIntMathService>();
            api.Method.Get("/stringtest/{stringValue}", (string stringValue) => stringValue + " Hello World!");
        }
    }

    public class ExposeClassTest2 : BaseRequestTest
    {
        [Fact]
        public async Task ExposeAttributePost()
        {
            var response = await Post("/AddGenericResult", new { a = 2, b = 2 });

            var result = await Deserialize<GenericResult<int>>(response);

            Assert.NotNull(result);
            Assert.Equal(4, result.Result);
        }

        [Fact]
        public async Task ExposeAttributeGet()
        {
            var response = await Get("/noparams2");

            var result = await Deserialize<GenericResult<int>>(response);

            Assert.NotNull(result);
            Assert.NotEqual(0, result.Result);
        }
        
        protected override void ConfigureServices(IServiceCollection services)
        {
            base.ConfigureServices(services);
            services.AddRouting();
        }

        protected override void ConfigureAspNetPipeline(IApplicationBuilder app)
        {
            app.UseRouting();
            base.ConfigureAspNetPipeline(app);
        }

        protected override void ApiRegistration(IRpcApi api)
        {
            api.Environment.UseAspNetRouting();
            api.Expose<AttributedIntMathService>();
            api.Method.Get("/noparams2", () => new GenericResult<int> { Result = 10 });
        }
    }
}
