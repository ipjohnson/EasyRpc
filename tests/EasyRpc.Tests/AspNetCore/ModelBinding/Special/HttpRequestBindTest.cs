using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using EasyRpc.AspNetCore;
using Microsoft.AspNetCore.Http;
using Xunit;

namespace EasyRpc.Tests.AspNetCore.ModelBinding.Special
{
    public class HttpRequestBindTest : BaseRequestTest
    {
        #region Service

        public class Service
        {
            public async Task Test(HttpRequest request, HttpResponse response)
            {
                var model = await request.Deserialize<Model>();

                model.Value += " append";

                await response.Serialize(model);
            }
        }


        public class Model
        {
            public string Value { get; set; }
        }

        #endregion

        #region Tests

        [Fact]
        public async Task ModelBinding_Special_RequestResponse()
        {
            var model = new Model {Value = "Test"};

            var response = await Post("/Service/Test", model);

            var result = await Deserialize<Model>(response);

            Assert.NotNull(result);
            Assert.Equal("Test append", result.Value);
        }

        #endregion

        #region registration

        protected override void ApiRegistration(IRpcApi api)
        {
            api.UseRequestExecutionContextFeature();

            api.Expose<Service>();
        }

        #endregion
    }
}
