using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace EasyRpc.Tests.DynamicClient.Post
{
    public class IntReturnPostBodyTest : BaseDynamicClientTest
    {
        #region Test

        [Fact]
        public async Task SimplePostTest()
        {
            var service = GetService<ITestInterface>();

            var result = await service.Add(5, 10);

            Assert.Equal(15, result);
        }

        #endregion

        #region Interface
        public interface ITestInterface
        {
            Task<int> Add(int a, int b);
        }

        #endregion

        #region Service Definition

        /// <inheritdoc />
        protected override async Task<HttpResponseMessage> ProcessRequest(HttpRequestMessage requestMessage)
        {
            var request = await DeserializeRequest<RequestPayLoad>(requestMessage);
            
            return await SerializeResponse(request.a + request.b);
        }

        public class RequestPayLoad
        {
            public int a { get; set; }

            public int b { get; set; }
        }
        #endregion
    }
}
