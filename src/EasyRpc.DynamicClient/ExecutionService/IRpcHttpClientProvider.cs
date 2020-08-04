using System;
using System.Collections.Generic;
using System.Text;

namespace EasyRpc.DynamicClient.ExecutionService
{
    public interface IRpcHttpClientProvider
    {
        IRpcHttpClient ProvideClient();
    }
}
