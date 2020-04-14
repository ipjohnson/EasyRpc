using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using EasyRpc.AspNetCore;
using EasyRpc.Tests.Services.Models;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace EasyRpc.Tests.AspNetCore.ModelBinding.AspNetRouting
{

    public class AspNetGetDoubleRouteTests : BaseRequestTest
    {
        #region Tests

        [Fact]
        public async Task ModelBinding_AspNetRouting_GetDouble()
        {
            var response = await Get("/TestPath/123.456");

            var value = await Deserialize<GenericResult<double>>(response);

            Assert.NotNull(value);
            Assert.Equal(123.456, value.Result);
        }

        #endregion

        #region Registration

        protected override void ConfigureServices(IServiceCollection services)
        {
            base.ConfigureServices(services);
            services.AddRouting();
        }

        protected override void ApiRegistration(IApiConfiguration api)
        {
            api.Configure.UseAspNetRouting();
            api.GetMethod("/TestPath/{doubleParam}", (double doubleParam) => doubleParam);
        }

        #endregion
    }
}
