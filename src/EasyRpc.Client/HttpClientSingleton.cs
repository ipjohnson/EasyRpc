using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace EasyRpc.Client
{
    public interface IHttpClientSingleton
    {

    }

    public class HttpClientSingleton : IHttpClientSingleton, IDisposable
    {
        private HttpClient _httpClient;

        public HttpClientSingleton(string url)
        {
            _httpClient = new HttpClient { BaseAddress = new Uri(url) };
        }

        /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
