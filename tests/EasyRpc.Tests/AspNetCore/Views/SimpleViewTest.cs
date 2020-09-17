using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using EasyRpc.Abstractions.Path;
using EasyRpc.AspNetCore;
using EasyRpc.AspNetCore.Views;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace EasyRpc.Tests.AspNetCore.Views
{
    public class SimpleViewTest : BaseRequestTest
    {
        #region Service

        public class Service
        {
            [ReturnView]
            [GetMethod("/Simple/Index")]
            public List<int> Index()
            {
                return new List<int> { 1, 2, 3, 4, 5 };
            }
        }

        #endregion

        #region Tests

        [Fact]
        public async Task Views_SimpleViewTest()
        {
            var response = await Get("/Simple/Index");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var result = await response.Content.ReadAsStringAsync();

            for (var i = 1; i <= 5; i++)
            {
                Assert.Contains($"<div>{i}</div>", result);
            }
        }

        #endregion

        #region Registration

        /// <inheritdoc />
        protected override void ConfigureServices(IServiceCollection services)
        {
            base.ConfigureServices(services);

            services.AddMvc();
        }

        protected override void ApiRegistration(IApiConfiguration api)
        {
            api.Expose<Service>();
        }

        #endregion
    }
}
