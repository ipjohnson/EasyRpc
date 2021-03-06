﻿using System;
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
    public class AspNetGetIntRouteTests : BaseRequestTest
    {
        #region Tests

        [Fact]
        public async Task ModelBinding_AspNetRouting_GetInt()
        {
            var response = await Get("/TestPath/123");

            var value = await Deserialize<int>(response);

            Assert.Equal(123, value);
        }

        #endregion

        #region Registration

        protected override void ConfigureServices(IServiceCollection services)
        {
            base.ConfigureServices(services);
            services.AddRouting();
        }
        
        protected override bool UseInternalRouting => false;

        protected override void ApiRegistration(IRpcApi api)
        {
            api.Method.Get("/TestPath/{intParam}", (int intParam) => intParam);
        }

        #endregion
    }
}
