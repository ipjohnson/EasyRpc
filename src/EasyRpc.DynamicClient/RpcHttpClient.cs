using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace EasyRpc.DynamicClient
{
    public class RpcHttpClient : IRpcHttpClient
    {
        private readonly HttpClient _client;

        public RpcHttpClient(HttpClient client, bool callByParameterName, int timeout)
        {
            _client = client;
            CallByParameterName = callByParameterName;
            Timeout = timeout;
        }

        public bool CallByParameterName { get; }

        public int Timeout { get; }

        public Task<HttpResponseMessage> SendAsync(HttpRequestMessage message)
        {
            return _client.SendAsync(message);
        }
    }
}
