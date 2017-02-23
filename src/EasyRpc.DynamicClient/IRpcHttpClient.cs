using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace EasyRpc.DynamicClient
{
    public interface IRpcHttpClient
    {
        bool CallByParameterName { get; }

        int Timeout { get; }
        
        Task<HttpResponseMessage> SendAsync(HttpRequestMessage message);
    }
}
