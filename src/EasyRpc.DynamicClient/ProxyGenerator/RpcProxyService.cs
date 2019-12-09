using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
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
        void MakeCallNoReturn(string @namespace, string className, string methodName, byte[] bytes, bool compressRequest, bool compressResponse);

        Task MaskAsyncCallNoReturn(string @namespace, string className, string methodName, byte[] bytes, bool compressRequest, bool compressResponse);

        Task<T> MakeAsyncCallWithReturn<T>(string @namespace, string className, string methodName, byte[] bytes, bool compressRequest, bool compressResponse);

        T MakeCallWithReturn<T>(string @namespace, string className, string methodName, byte[] bytes, bool compressRequest, bool compressResponse);
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

        public virtual void MakeCallNoReturn(string @namespace, string className, string methodName, byte[] bytes, bool compressRequest, bool compressResponse)
        {
            var result = MakeAsyncCallWithReturn<object>(@namespace, className, methodName, bytes, compressRequest, compressResponse).Result;
        }

        public virtual Task MaskAsyncCallNoReturn(string @namespace, string className, string methodName, byte[] bytes, bool compressRequest, bool compressResponse)
        {
            return MakeAsyncCallWithReturn<object>(@namespace, className, methodName, bytes, compressRequest, compressResponse);
        }

        public virtual async Task<T> MakeAsyncCallWithReturn<T>(string @namespace, string className, string methodName, byte[] bytes, bool compressRequest, bool compressResponse)
        {
            var response = await SendByteArray(@namespace, className, methodName, bytes, compressRequest, compressResponse).ConfigureAwait(false);
            var stream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);

            foreach (var headerProcessorse in _headerProcessors)
            {
                headerProcessorse.ProcessResponseHeader(response);
            }

            if (response.Content.Headers.TryGetValues("Content-Encoding", out var encoding) )
            {
                if (encoding.Contains("gzip"))
                {
                    using (var gzipStream = new GZipStream(stream, CompressionMode.Decompress))
                    {
                        return ResponseMessageResult<T>(methodName, gzipStream);
                    }
                }

                throw new Exception("Received unknown Content-Encoding: " + encoding);
            }

            return ResponseMessageResult<T>(methodName, stream);
        }

        private T ResponseMessageResult<T>(string methodName, Stream stream)
        {
            using (var streamReader = new StreamReader(stream))
            {
                using (var jsonReader = new JsonTextReader(streamReader))
                {
                    var responseMessage = _jsonSerializer.Deserialize<RpcResponseMessage<T>>(jsonReader);

                    if (responseMessage.Error == null)
                    {
                        return responseMessage.Result;
                    }

                    if (responseMessage.Error.Code == (int) JsonRpcErrorCode.MethodNotFound)
                    {
                        throw new MethodNotFoundException(methodName);
                    }

                    if (responseMessage.Error.Code == (int) JsonRpcErrorCode.UnauthorizedAccess)
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

        public virtual T MakeCallWithReturn<T>(string @namespace, string className, string methodName, byte[] bytes, bool compressRequest, bool compressResponse)
        {
            return  MakeAsyncCallWithReturn<T>(@namespace, className, methodName, bytes, compressRequest, compressResponse).Result;
        }

        protected virtual async Task<HttpResponseMessage> SendByteArray(string @namespace, string className,
            string methodName, byte[] bytes, bool compressRequest, bool compressResponse)
        {
            var httpRequest =
                new HttpRequestMessage(HttpMethod.Post, className)
                {
                    Content = new ByteArrayContent(bytes)
                };

            httpRequest.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json") { CharSet = "utf-8"};

            if (compressRequest)
            {
                httpRequest.Content.Headers.Add("Content-Encoding", "gzip");
            }

            if (compressResponse)
            {
                httpRequest.Headers.Add("Accept-Encoding", "gzip");
            }

            foreach (var headerProcessorse in _headerProcessors)
            {
                headerProcessorse.ProcessRequestHeader(httpRequest);
            }

            var client = _clientProvider.GetHttpClient(@namespace, className);
            
            var response = await client.SendAsync(httpRequest).ConfigureAwait(false);

            if (response.IsSuccessStatusCode)
            {
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
