using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using EasyRpc.Abstractions.Path;
using EasyRpc.AspNetCore;
using Xunit;

namespace EasyRpc.Tests.AspNetCore.Routing.Internal
{
    public class NonBoundRouteTokens : BaseRequestTest
    {
        #region Service

        public class Service
        {
            [GetMethod("/Service/Get/{id}/{secondId}")]
            public string Get(int id, RequestExecutionContext context)
            {
                return $"{id}-{context.Parameters["secondId"]}";
            }

            [GetMethod("/Service/GetInt/{id}/{secondId:int}")]
            public int GetInt(int id, RequestExecutionContext context)
            {
                return id + (int)context.Parameters["secondId"];
            }
        }

        #endregion

        #region Tests

        [Fact]
        public async Task Routing_Internal_NonBoundRouteTokens()
        {
            var response = await Get("/Service/Get/10/5");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var result = await Deserialize<string>(response);

            Assert.Equal("10-5", result);
        }


        [Fact]
        public async Task Routing_Internal_NonBoundRouteTokens_Typed()
        {
            var response = await Get("/Service/GetInt/10/5");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var result = await Deserialize<int>(response);

            Assert.Equal(15, result);
        }
        #endregion

        #region registration

        protected override void ApiRegistration(IApiConfiguration api)
        {
            api.Expose<Service>();
        }

        #endregion
    }
}
