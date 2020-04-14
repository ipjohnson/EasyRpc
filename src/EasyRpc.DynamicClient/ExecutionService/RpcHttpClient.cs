using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EasyRpc.DynamicClient.ExecutionService
{
    public interface IRpcHttpClient
    {
        Task<HttpResponseMessage> SendAsync(HttpRequestMessage message,
            CancellationToken? cancellationToken);
    }

    public class RpcHttpClient : IRpcHttpClient
    {
        private HttpClient _httpClient;


        public Task<HttpResponseMessage> SendAsync(HttpRequestMessage message,
            CancellationToken? cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
