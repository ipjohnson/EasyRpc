﻿using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using EasyRpc.AspNetCore;
using Xunit;

namespace EasyRpc.Tests.AspNetCore.ResponseHeaders
{
    public class GetMethodGlobalNoCache : BaseRequestTest
    {
        private static readonly string _cacheControl = "no-cache";

        #region Tests

        [Fact]
        public async Task ResponseHeaders_GetMethodGlobalNoCache()
        {
            var response = await Get("/test");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var headerFound = response.Headers.TryGetValues("Cache-Control", out var values);

            Assert.True(headerFound);
            Assert.Contains(values, value => value == _cacheControl);
        }

        #endregion

        #region Registration

        protected override void ApiRegistration(IRpcApi api)
        {
            api.Header("Cache-Control", _cacheControl);
            api.Method.Get("/test",() => DateTime.Now.ToString());
        }

        #endregion
    }
}
