using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using EasyRpc.Abstractions.Path;
using EasyRpc.AspNetCore;
using Microsoft.AspNetCore.Http;
using Xunit;

namespace EasyRpc.Tests.AspNetCore.Options
{
    public class SpecificPathOptionsTest : BaseRequestTest
    {
        #region Service

        public class Service
        {
            [GetMethod]
            [PostMethod]
            public int GetValue()
            {
                return 5;
            }
        }

        #endregion

        #region Tests

        [Fact]
        public async Task Options_SpecificMatchingPath()
        {
            var response = await SendAsync(HttpMethods.Options, "/Service/GetValue");

            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

            Assert.True(response.Content.Headers.TryGetValues("Allow", out var options));

            var allOptions = options.ToList();

            Assert.Contains(HttpMethods.Get, allOptions);
            Assert.Contains(HttpMethods.Post, allOptions);
            Assert.DoesNotContain(HttpMethods.Patch, allOptions);
            Assert.DoesNotContain(HttpMethods.Put, allOptions);
            Assert.DoesNotContain(HttpMethods.Delete, allOptions);
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
