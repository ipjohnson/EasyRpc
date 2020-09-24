using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using EasyRpc.Abstractions.Path;
using EasyRpc.AspNetCore;
using Xunit;

namespace EasyRpc.Tests.AspNetCore.Head
{
    public class HeadMethodTests : BaseRequestTest
    {
        #region Service

        public class Service
        {
            [GetMethod]
            public Model Get(int id)
            {
                return id < 10 ?
                    null :
                    new Model { Value = $"id:{id}" };
            }
        }

        public class Model
        {
            public string Value { get; set; }
        }

        #endregion

        #region Tests

        [Fact]
        public async Task Head_ValidRequest()
        {
            var response = await SendAsync(HttpMethod.Head, "/Service/Get/15");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }
        
        [Fact]
        public async Task Head_InvalidRequest()
        {
            var response = await SendAsync(HttpMethod.Head, "/Service/Get/1");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        #endregion

        #region Registration

        protected override void ApiRegistration(IApiConfiguration api)
        {
            api.Expose<Service>();
        }

        #endregion
    }
}
