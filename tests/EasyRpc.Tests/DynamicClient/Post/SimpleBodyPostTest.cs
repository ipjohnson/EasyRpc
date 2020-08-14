using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace EasyRpc.Tests.DynamicClient.Post
{
    public class SimpleBodyPostTest : BaseDynamicClientTest
    {
        #region Test

        [Fact]
        public async Task SimplePostTest()
        {
            var service = GetService<ITestInterface>();

            var result = await service.Add(5, 10);

            Assert.Equal(15, result.Result);
        }

        #endregion

        #region Interface
        public interface ITestInterface
        {
            Task<WrapperInt> Add(int a, int b);
        }

        public class WrapperInt
        {
            public int Result { get; set; }
        }
        #endregion

        #region Service Definition

        /// <inheritdoc />
        protected override async Task<HttpResponseMessage> ProcessRequest(HttpRequestMessage requestMessage)
        {
            var request = await DeserializeRequest<RequestPayLoad>(requestMessage);

            var response = new WrapperInt { Result = request.a + request.b };

            return await SerializeResponse(response);
        }
        
        public class RequestPayLoad
        {
            public int a { get; set; }

            public int b { get; set; }
        }
        #endregion
    }
}
