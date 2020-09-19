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
    public class TaskResultTests : BaseRequestTest
    {
        #region Service Class

        public class Services
        {
            [PostMethod("/teststring/{pathValue}")]
            public async Task TestString(ISharedStorage sharedStorage, string pathValue, string id, string id2)
            {
                sharedStorage.Items["id"] = pathValue + id + id2;

                await Task.Delay(1);
            }

            [PostMethod("/TestModel")]
            public async Task TestModel(ISharedStorage sharedStorage, StringResult body)
            {
                sharedStorage.Items["body"] = body;

                await Task.Delay(1);
            }
        }

        #endregion

        #region Tests

        [Fact]
        public async Task TaskResultTest()
        {
            var id = " Hello";
            var id2 = " World!";

            var response = await Post("/teststring/Yeah", new { id, id2 });

            Assert.True(response.IsSuccessStatusCode);

            Assert.Equal("Yeah" + id + id2, Shared.Items["id"]);
        }

        [Fact]
        public async Task BodyTest()
        {
            var stringResult = new StringResult{ Result = "Hello World!" };

            var response = await Post("/TestModel", stringResult);

            Assert.True(response.IsSuccessStatusCode);

            Assert.Equal(stringResult.Result, ((StringResult)Shared.Items["body"]).Result);
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
