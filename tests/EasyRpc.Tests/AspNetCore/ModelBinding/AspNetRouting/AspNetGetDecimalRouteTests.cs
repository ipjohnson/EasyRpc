﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using EasyRpc.AspNetCore;
using EasyRpc.Tests.Services.Models;
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

            var value = await Deserialize<GenericResult<decimal>>(response);

            Assert.NotNull(value);
            Assert.Equal(123.456m, value.Result);
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
            api.GetMethod("/TestPath/{decimalParam}", (decimal decimalParam) => decimalParam);
        }

        #endregion
    }
}