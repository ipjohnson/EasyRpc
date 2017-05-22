namespace EasyRpc.DynamicClient
{
    public interface IRpcHttpClientProvider
    {
        IRpcHttpClient GetHttpClient(string className);

        void ReturnHttpClient(string className, IRpcHttpClient client);
    }
}
