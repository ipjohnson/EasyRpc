using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using EasyRpc.AspNetCore;
using Xunit;

namespace EasyRpc.Tests.AspNetCore.Serialization.SystemTextJson
{
    public class LargeMessagePostTest : BaseRequestTest
    {

        #region Service

        public class Service
        {
            public Task<List<Model>> List(List<Model> input)
            {
                return Task.FromResult(input);
            }
        }

        public class Model
        {
            public string StringValue { get; set; }
        }

        #endregion

        #region Tests

        [Fact]
        public async Task Serialization_SystemTextJson_LargeMessagePost()
        {
            var list = new List<Model>();

            for (int i = 0; i < 100000; i++)
            {
                list.Add(new Model{ StringValue = "Model String " + i});
            }

            var response = await Post("/service/list", list);

            var result = await Deserialize<List<Model>>(response);

            Assert.NotNull(result);
            Assert.Equal(list.Count, result.Count);
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
