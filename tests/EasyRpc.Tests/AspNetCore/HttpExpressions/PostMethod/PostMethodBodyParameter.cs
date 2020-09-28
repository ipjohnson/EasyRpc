using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using EasyRpc.AspNetCore;
using EasyRpc.Tests.Services.Models;
using Xunit;

namespace EasyRpc.Tests.AspNetCore.HttpExpressions.PostMethod
{
    public class PostMethodBodyParameter : BaseRequestTest
    {

        #region Tests

        [Fact]
        public async Task HttpExpressions_PostMethodBody_SimplePost()
        {
            var valueA = 5;
            var valueB = 10;

            var response = await Post(_simpleBodyPath, new { valueA, valueB });

            var value = await Deserialize<int>(response);

            Assert.Equal(valueA + valueB, value);
        }

        #endregion

        #region Registration

        protected override void ApiRegistration(IRpcApi api)
        {
            api.PostMethod(_simpleBodyPath, (RequestBody body) => body.ValueA + body.ValueB);
        }

        #endregion

        #region Paths

        private const string _simpleBodyPath = "/SimplePostBody";

        #endregion

        #region Models

        public class RequestBody
        {
            public int ValueA { get; set; }

            public int ValueB { get; set; }
        }

        #endregion
    }
}
