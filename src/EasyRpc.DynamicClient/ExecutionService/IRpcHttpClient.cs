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
            CancellationToken cancellationToken);
    }
}
