using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;

namespace EasyRpc.DynamicClient
{
    public interface IHeaderProcessor
    {
        void ProcessRequestHeader(HttpRequestMessage message);

        void ProcessResponseHeader(HttpResponseMessage message);
    }
}
