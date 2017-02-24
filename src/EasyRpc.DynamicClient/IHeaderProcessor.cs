using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace EasyRpc.DynamicClient
{
    public interface IHeaderProcessor
    {
        void ProcessRequestHeader(HttpRequestMessage message);

        void ProcessResponseHeader(HttpResponseMessage message);
    }
}
