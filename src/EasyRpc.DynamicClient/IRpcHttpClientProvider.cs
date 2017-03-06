using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace EasyRpc.DynamicClient
{
    public interface IRpcHttpClientProvider
    {
        IRpcHttpClient GetHttpClient(string className);

        void ReturnHttpClient(string className, IRpcHttpClient client);
    }
}
