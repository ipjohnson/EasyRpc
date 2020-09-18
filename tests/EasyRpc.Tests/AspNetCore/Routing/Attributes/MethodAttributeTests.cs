using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using EasyRpc.Abstractions.Path;
using EasyRpc.AspNetCore;
using Microsoft.AspNetCore.Http;
using Xunit;

namespace EasyRpc.Tests.AspNetCore.Routing.Attributes
{
    public class MethodAttributeTests : BaseRequestTest
    {
        #region Service

        public class Service
        {
            [PostMethod("/Service/BodyModel/{modelId}")]
            [PutMethod("/Service/BodyModel/{modelId}")]
            [PatchMethod("/Service/BodyModel/{modelId}")]
            public Model BodyModel(int modelId, Model model)
            {
                model.Id = modelId;

                return model;
            }
        }

        public class Model
        {
            public string Value { get; set; }
            public int Id { get; set; }
        }
        #endregion

        #region Tests

        [Theory]
        [InlineData("POST")]
        [InlineData("PATCH")]
        [InlineData("PUT")]
        public async Task Routing_Attributes_BodyMethod(string httpMethod)
        {
            var id = 20;

            var response = await SendAsync(httpMethod, $"/Service/BodyModel/{id}", new Model {Value = "Test"});

            var result = await Deserialize<Model>(response);

            Assert.NotNull(result);
            Assert.Equal(id, result.Id);
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
