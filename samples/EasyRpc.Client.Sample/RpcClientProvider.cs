using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using EasyRpc.DynamicClient;

namespace EasyRpc.Client.Sample
{
    public class RpcClientProvider : IRpcHttpClientProvider
    {
        private HttpClient _httpClient = new HttpClient { BaseAddress = new Uri("http://localhost:5000/RpcApi/") };

        public IRpcHttpClient GetHttpClient(Type type)
        {
            return new RpcHttpClient(_httpClient, true, 30);
        }

        public void ReturnHttpClient(Type type, IRpcHttpClient client)
        {

        }
    }
}
