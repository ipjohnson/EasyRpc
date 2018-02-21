using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using EasyRpc.DynamicClient.Exceptions;
using EasyRpc.DynamicClient.Messages;
using Newtonsoft.Json;

namespace EasyRpc.DynamicClient.ProxyGenerator
{
    public interface IRpcProxyService
    {
        void MakeCallNoReturn(string @namespace, string className, string methodName, byte[] bytes);

        Task MaskAsyncCallNoReturn(string @namespace, string className, string methodName, byte[] bytes);

        Task<T> MakeAsyncCallWithReturn<T>(string @namespace, string className, string methodName, byte[] bytes);

        T MakeCallWithReturn<T>(string @namespace, string className, string methodName, byte[] bytes);
    }

    public class RpcProxyService : IRpcProxyService
    {
        private readonly IRpcHttpClientProvider _clientProvider;
        private readonly IHeaderProcessor[] _headerProcessors;
        private readonly JsonSerializer _jsonSerializer;

        public RpcProxyService(IRpcHttpClientProvider clientProvider, IHeaderProcessor[] headerProcessors, JsonSerializer jsonSerializer)
        {
            _clientProvider = clientProvider;
            _headerProcessors = headerProcessors;
            _jsonSerializer = jsonSerializer;
        }

        public virtual void MakeCallNoReturn(string @namespace, string className, string methodName, byte[] bytes)
        {
            var result = MakeAsyncCallWithReturn<object>(@namespace, className, methodName, bytes).Result;
        }

        public virtual Task MaskAsyncCallNoReturn(string @namespace, string className, string methodName, byte[] bytes)
        {
            return MakeAsyncCallWithReturn<object>(@namespace, className, methodName, bytes);
        }

        public virtual async Task<T> MakeAsyncCallWithReturn<T>(string @namespace, string className, string methodName, byte[] bytes)
        {
            var response = await SendByteArray(@namespace, className, methodName, bytes).ConfigureAwait(false);

            using (var streamReader = new StreamReader(await response.Content.ReadAsStreamAsync().ConfigureAwait(false)))
            {
                using (var jsonReader = new JsonTextReader(streamReader))
                {
                    var responseMessage = _jsonSerializer.Deserialize<RpcResponseMessage<T>>(jsonReader);

                    if (responseMessage.Error == null)
                    {
                        return responseMessage.Result;
                    }

                    if (responseMessage.Error.Code == (int)JsonRpcErrorCode.MethodNotFound)
                    {
                        throw new MethodNotFoundException(methodName);
                    }

                    if (responseMessage.Error.Code == (int)JsonRpcErrorCode.UnauthorizedAccess)
                    {
                        throw new UnauthorizedMethodException(methodName);
                    }

                    if (responseMessage.Error.Code == (int) JsonRpcErrorCode.InvalidRequest)
                    {
                        throw new InvalidRequestException(methodName, responseMessage.Error.Message);
                    }

                    throw new InternalServerErrorException(methodName, responseMessage.Error.Message);
                }
            }
        }

        public virtual T MakeCallWithReturn<T>(string @namespace, string className, string methodName, byte[] bytes)
        {
            return  MakeAsyncCallWithReturn<T>(@namespace, className, methodName, bytes).Result;
        }

        protected virtual async Task<HttpResponseMessage> SendByteArray(string @namespace, string className, string methodName, byte[] bytes)
        {
            var httpRequest =
                new HttpRequestMessage(HttpMethod.Post, className)
                {
                    Content = new ByteArrayContent(bytes)
                };

            httpRequest.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json") { CharSet = "utf-8"};

            foreach (var headerProcessorse in _headerProcessors)
            {
                headerProcessorse.ProcessRequestHeader(httpRequest);
            }

            var client = _clientProvider.GetHttpClient(@namespace, className);

            var response = await client.SendAsync(httpRequest).ConfigureAwait(false);

            if (response.IsSuccessStatusCode)
            {
                foreach (var headerProcessorse in _headerProcessors)
                {
                    headerProcessorse.ProcessResponseHeader(response);
                }

                return response;
            }

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                throw new UnauthorizedMethodException(methodName);
            }

            throw new DynamicMethodException(methodName, $"Error response status {response.StatusCode}");
        }
    }
}
