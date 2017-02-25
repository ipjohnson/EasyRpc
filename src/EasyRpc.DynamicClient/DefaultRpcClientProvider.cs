using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace EasyRpc.DynamicClient
{
    public class DefaultRpcClientProvider : IRpcHttpClientProvider, IDisposable
    {
        private readonly bool _callByNamedParameter;
        private readonly int _timeout;
        private HttpClient _client;

        public DefaultRpcClientProvider(string url, bool callByNamedParameter = false, int timeout = 30)
        {
            _callByNamedParameter = callByNamedParameter;
            _timeout = timeout;
            _client = new HttpClient { BaseAddress = new Uri(url) };
        }

        public IRpcHttpClient GetHttpClient(Type type)
        {
            return new RpcHttpClient(_client, _callByNamedParameter, _timeout);
        }

        public void ReturnHttpClient(Type type, IRpcHttpClient client)
        {

        }

        public void Dispose()
        {
            var client = Interlocked.Exchange(ref _client, null);

            client?.Dispose();
        }
    }
}
