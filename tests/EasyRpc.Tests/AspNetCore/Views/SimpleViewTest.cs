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


            [ReturnView(ViewName = "~/Views/Simple/Count.cshtml")]
            [GetMethod("/Simple/Count/{count}")]
            public List<int> Count(int count)
            {
                var returnList = new List<int>();

                for (int i = 1; i <= count; i++)
                {
                    returnList.Add(i);
                }
                return returnList;
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


        [Fact]
        public async Task Views_SimpleView_With_Parameter()
        {
            var count = 10;
            var response = await Get($"/Simple/Count/{count}");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var result = await response.Content.ReadAsStringAsync();

            Assert.Contains($"<div>Count={count}</div>", result);

            for (var i = 1; i <= count; i++)
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

        protected override void ApiRegistration(IRpcApi api)
        {
            api.Expose<Service>();
        }

        #endregion
    }
}
