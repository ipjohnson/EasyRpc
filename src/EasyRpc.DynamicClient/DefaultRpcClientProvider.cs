using System;
using System.Net.Http;
using System.Threading;

namespace EasyRpc.DynamicClient
{
    public class DefaultRpcClientProvider : IRpcHttpClientProvider, IDisposable
    {
        private readonly int _timeout;
        private HttpClient _client;

        public DefaultRpcClientProvider(string url, int timeout = 30)
        {
            _timeout = timeout;
            _client = new HttpClient { BaseAddress = new Uri(url) };
        }

        public virtual IRpcHttpClient GetHttpClient(string @namespace, string className)
        {
            return new RpcHttpClient(_client, _timeout);
        }

        public virtual void ReturnHttpClient(string @namespace, string className, IRpcHttpClient client)
        {

        }

        public void Dispose()
        {
            var client = Interlocked.Exchange(ref _client, null);

            client?.Dispose();
        }
    }
}
