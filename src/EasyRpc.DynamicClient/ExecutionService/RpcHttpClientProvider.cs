using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;

namespace EasyRpc.DynamicClient.ExecutionService
{
    public class RpcHttpClientProvider : IRpcHttpClientProvider
    {
        private readonly HttpClient _client;

        public RpcHttpClientProvider(HttpClient client)
        {
            _client = client;
        }

        /// <inheritdoc />
        public IRpcHttpClient ProvideClient()
        {
            return new RpcHttpClient(_client);
        }
    }
}
