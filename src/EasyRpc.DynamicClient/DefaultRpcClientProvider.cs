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
        private readonly int _timeout;
        private HttpClient _client;

        public DefaultRpcClientProvider(string url, int timeout = 30)
        {
            _timeout = timeout;
            _client = new HttpClient { BaseAddress = new Uri(url) };
        }

        public virtual IRpcHttpClient GetHttpClient(string className)
        {
            return new RpcHttpClient(_client, _timeout);
        }

        public virtual void ReturnHttpClient(string className, IRpcHttpClient client)
        {

        }

        public void Dispose()
        {
            var client = Interlocked.Exchange(ref _client, null);

            client?.Dispose();
        }
    }
}
