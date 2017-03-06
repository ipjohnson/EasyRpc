using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace EasyRpc.DynamicClient
{
    public class RpcHttpClient : IRpcHttpClient
    {
        private readonly HttpClient _client;

        public RpcHttpClient(HttpClient client, int timeout)
        {
            _client = client;
            Timeout = timeout;
        }
        
        public int Timeout { get; }

        public Task<HttpResponseMessage> SendAsync(HttpRequestMessage message)
        {
            var cancellationTokenSource = new CancellationTokenSource(Timeout * 1000);

            return _client.SendAsync(message, cancellationTokenSource.Token);
        }
    }
}
