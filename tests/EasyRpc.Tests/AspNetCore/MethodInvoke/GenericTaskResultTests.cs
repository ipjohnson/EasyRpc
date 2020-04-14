using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using EasyRpc.Abstractions.Path;
using EasyRpc.AspNetCore;
using EasyRpc.Tests.Services.Models;
using Xunit;

namespace EasyRpc.Tests.AspNetCore.MethodInvoke
{
    public class GenericTaskResultTests : BaseRequestTest
    {
        #region Service Class

        public class Services
        {
            [PostMethod("/TestString")]
            public async Task<StringResult> TestString(string id)
            {
                return new StringResult { Result = id };
            }

            [PostMethod("/WrappedTestString")]
            public async Task<string> WrappedTestString(string id)
            {
                return id;
            }
        }

        #endregion

        #region Tests

        [Fact]
        public async Task TaskResultTest()
        {
            var id = "Hello";

            var response = await Post("/teststring", new { id });

            var value = await Deserialize<GenericResult<string>>(response);

            Assert.NotNull(value);
            Assert.Equal(id, value.Result);
        }


        [Fact]
        public async Task WrappedTaskResultTest()
        {
            var id = "Hello";

            var response = await Post("/WrappedTestString", new { id });

            var value = await Deserialize<GenericResult<string>>(response);


            Assert.NotNull(value);
            Assert.Equal(id, value.Result);
        }

        #endregion

        #region Api Registration

        protected override void ApiRegistration(IApiConfiguration api)
        {
            api.Expose<Services>();
        }

        #endregion
    }
}
