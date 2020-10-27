using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using EasyRpc.Abstractions.Binding;
using EasyRpc.Abstractions.Path;
using EasyRpc.AspNetCore;
using Xunit;

namespace EasyRpc.Tests.AspNetCore.ModelBinding.InstanceBinding
{
    public class BindFromHeaderInstanceTest : BaseRequestTest
    {
        #region Service

        public class Service
        {
            [BindFromHeader]
            public string HeaderValue { get; set; }

            [GetMethod]
            public string Get()
            {
                return HeaderValue;
            }
        }

        #endregion

        #region Tests

        [Fact]
        public async Task ModelBinding_InstanceBinding_BindFromHeader()
        {
            var headerValue = "Hello";

            var response = await SendAsync(HttpMethod.Get, "/Service/Get", headers: new { HeaderValue = headerValue });

            var result = await Deserialize<string>(response);

            Assert.Equal(headerValue, result);
        }

        #endregion

        #region Registration

        protected override void ApiRegistration(IRpcApi api)
        {
            api.Expose<Service>();
        }

        #endregion
    }
}
