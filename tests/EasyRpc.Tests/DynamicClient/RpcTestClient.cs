using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using EasyRpc.DynamicClient.ExecutionService;

namespace EasyRpc.Tests.DynamicClient
{
    public class RpcTestClient : IRpcHttpClient
    {
        private Func<HttpRequestMessage, Task<HttpResponseMessage>> _clientFunc;

        public RpcTestClient(Func<HttpRequestMessage, Task<HttpResponseMessage>> clientFunc)
        {
            _clientFunc = clientFunc;
        }


        /// <inheritdoc />
        public Task<HttpResponseMessage> SendAsync(HttpRequestMessage message, CancellationToken cancellationToken)
        {
            return _clientFunc(message);
        }
    }
}
