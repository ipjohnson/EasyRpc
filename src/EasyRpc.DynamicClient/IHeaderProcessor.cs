using System.Net.Http;

namespace EasyRpc.DynamicClient
{
    public interface IHeaderProcessor
    {
        void ProcessRequestHeader(HttpRequestMessage message);

        void ProcessResponseHeader(HttpResponseMessage message);
    }
}
