using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using EasyRpc.Abstractions.Path;
using EasyRpc.Abstractions.Response;
using EasyRpc.AspNetCore;
using Xunit;

namespace EasyRpc.Tests.AspNetCore.Serialization.Raw
{
    public class RawStringTest : BaseRequestTest
    {
        private static readonly string _returnString = "Hello, World!";

        #region Tests

        [Fact]
        public async Task Serialization_Raw_StringTest()
        {
            var response = await Get("/plaintext");

            var stringValue = await response.Content.ReadAsStringAsync();

            Assert.Equal(stringValue, _returnString);
        }

        #endregion

        #region registration

        protected override void ApiRegistration(IRpcApi api)
        {
            api.Expose<Service>();
        }

        #endregion

        #region Service

        public class Service
        {
            [RawContent("text/plain")]
            [GetMethod("/plaintext")]
            public string PlainText()
            {
                return _returnString;
            }
        }

        #endregion
    }
}
