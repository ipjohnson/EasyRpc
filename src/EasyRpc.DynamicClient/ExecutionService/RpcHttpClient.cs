using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EasyRpc.DynamicClient.ExecutionService
{
    public class RpcHttpClient : IRpcHttpClient
    {
        private readonly HttpClient _httpClient;

        public RpcHttpClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }


        public Task<HttpResponseMessage> SendAsync(HttpRequestMessage message,
            CancellationToken cancellationToken)
        {
            return _httpClient.SendAsync(message, cancellationToken);
        }
    }
}
