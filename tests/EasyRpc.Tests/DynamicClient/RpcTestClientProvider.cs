using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using EasyRpc.DynamicClient.ExecutionService;

namespace EasyRpc.Tests.DynamicClient
{
    public class RpcTestClientProvider : IRpcHttpClientProvider
    {
        private Func<HttpRequestMessage, Task<HttpResponseMessage>> _clientFunc;

        public RpcTestClientProvider(Func<HttpRequestMessage, Task<HttpResponseMessage>> clientFunc)
        {
            _clientFunc = clientFunc;
        }

        /// <inheritdoc />
        public IRpcHttpClient ProvideClient()
        {
            return new RpcTestClient(_clientFunc);
        }
    }
}
