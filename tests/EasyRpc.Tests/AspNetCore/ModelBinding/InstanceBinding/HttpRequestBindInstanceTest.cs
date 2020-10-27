using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using EasyRpc.Abstractions.Path;
using EasyRpc.AspNetCore;
using Microsoft.AspNetCore.Http;
using Xunit;

namespace EasyRpc.Tests.AspNetCore.ModelBinding.InstanceBinding
{
    public class HttpRequestBindInstanceTest : BaseRequestTest
    {
        #region Service

        public class Service
        {
            public HttpContext Context { get; set; }

            [GetMethod]
            public int Get()
            {
                Assert.NotNull(Context);

                return 10;
            }

        }

        #endregion

        #region Tests

        [Fact]
        public async Task ModelBinding_InstanceBinding_HttpRequest()
        {
            var response = await Get("/Service/Get");

            var result = await Deserialize<int>(response);

            Assert.Equal(10, result);
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
