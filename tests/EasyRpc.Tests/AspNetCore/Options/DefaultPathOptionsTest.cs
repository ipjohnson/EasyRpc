using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using EasyRpc.Abstractions.Path;
using EasyRpc.AspNetCore;
using Microsoft.AspNetCore.Http;
using Xunit;

namespace EasyRpc.Tests.AspNetCore.Options
{
    public class DefaultPathOptionsTest : BaseRequestTest
    {
        #region Service

        public class Service
        {
            [GetMethod]
            public int GetValue()
            {
                return 5;
            }
        }

        #endregion

        #region Tests

        [Fact]
        public async Task Options_DefaultPath()
        {
            var response = await SendAsync(HttpMethod.Options, "*");

            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

            Assert.True(response.Content.Headers.TryGetValues("Allow", out var options));

            var allOptions = options.ToList();

            Assert.Contains(HttpMethods.Get, allOptions);
            Assert.Contains(HttpMethods.Post, allOptions);
            Assert.Contains(HttpMethods.Patch, allOptions);
            Assert.Contains(HttpMethods.Put, allOptions);
            Assert.Contains(HttpMethods.Delete, allOptions);
            Assert.Contains(HttpMethods.Options, allOptions);
        }

        #endregion

        #region registration

        protected override void ApiRegistration(IRpcApi api)
        {
            api.Expose<Service>();
        }

        #endregion
    }
}
