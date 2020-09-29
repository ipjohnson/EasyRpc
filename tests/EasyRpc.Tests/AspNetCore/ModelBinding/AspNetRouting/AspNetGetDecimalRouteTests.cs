using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using EasyRpc.AspNetCore;
using EasyRpc.Tests.Services.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace EasyRpc.Tests.AspNetCore.ModelBinding.AspNetRouting
{

    public class AspNetGetDecimalRouteTests : BaseRequestTest
    {
        #region Tests

        [Fact]
        public async Task ModelBinding_AspNetRouting_GetDecimal()
        {
            var response = await Get("/TestPath/123.456");

            var value = await Deserialize<decimal>(response);

            Assert.Equal(123.456m, value);
        }


        [Fact]
        public async Task ModelBinding_AspNetRouting_PostDecimal()
        {
            var response = await Post("/TestPath/123.456", new {});

            var value = await Deserialize<decimal>(response);

            Assert.Equal(123.456m, value);
        }

        #endregion

        #region Registration

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
            api.GetMethod("/TestPath/{decimalParam}", (decimal decimalParam) => decimalParam);
            api.PostMethod("/TestPath/{decimalParam}", (decimal decimalParam) => decimalParam);
        }

        #endregion
    }
}
