namespace EasyRpc.DynamicClient
{
    public interface IRpcHttpClientProvider
    {
        IRpcHttpClient GetHttpClient(string @namespace, string className);

        void ReturnHttpClient(string @namespace, string className, IRpcHttpClient client);
    }
}
