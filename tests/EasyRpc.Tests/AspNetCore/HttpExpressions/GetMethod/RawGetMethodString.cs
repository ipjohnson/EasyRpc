using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using EasyRpc.AspNetCore;
using Xunit;

namespace EasyRpc.Tests.AspNetCore.HttpExpressions.GetMethod
{
    public class RawGetMethodString : BaseRequestTest
    {
        private static readonly string _responseString = "Hello, World";

        #region Tests

        [Fact]
        public async Task HttpExpressions_GetMethod_RawGetMethodString()
        {
            var response = await Get("/plaintext");

            var value = await response.Content.ReadAsStringAsync();

            Assert.Equal(_responseString, value);
        }

        #endregion

        #region registration

        protected override void ApiRegistration(IRpcApi api)
        {
            api.GetMethod("/plaintext", () => _responseString).Raw("text/plain");
        }

        #endregion
    }
}
