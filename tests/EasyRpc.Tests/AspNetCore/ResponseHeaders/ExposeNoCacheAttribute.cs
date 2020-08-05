using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using EasyRpc.Abstractions.Headers;
using EasyRpc.Abstractions.Path;
using EasyRpc.AspNetCore;
using EasyRpc.Tests.Services.Models;
using Xunit;

namespace EasyRpc.Tests.AspNetCore.ResponseHeaders
{
    public class ExposeNoCacheAttribute : BaseRequestTest
    {
        private static string _cacheControl = "no-cache";

        #region Tests

        [Fact]
        public async Task ExposeNoCacheAttribute_Test()
        {
            var response = await Get("/Service/Now");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var value = await Deserialize<DateTime>(response);

            Assert.Equal(DateTime.Today, value.Date);

            var headerFound = response.Headers.TryGetValues("Cache-Control", out var values);

            Assert.True(headerFound);
            Assert.Contains(values, headerValue => headerValue == _cacheControl);
        }

        #endregion

        #region Registration

        protected override void ApiRegistration(IApiConfiguration api)
        {
            api.Expose<Service>();
        }

        #endregion


        #region Service

        public class Service
        {
            [GetMethod]
            [NoCache]
            public DateTime Now()
            {
                return DateTime.Now;
            }
        }

        #endregion

    }
}
