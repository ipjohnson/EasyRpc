using System.Net.Http;
using System.Threading.Tasks;

namespace EasyRpc.DynamicClient
{
    public interface IRpcHttpClient
    {
        //int Timeout { get; }
        
        Task<HttpResponseMessage> SendAsync(HttpRequestMessage message);
    }
}
