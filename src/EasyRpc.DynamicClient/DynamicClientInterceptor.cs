using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Castle.DynamicProxy;
using EasyRpc.DynamicClient.Exceptions;
using EasyRpc.DynamicClient.Messages;
using Newtonsoft.Json;

namespace EasyRpc.DynamicClient
{
    public class DynamicClientInterceptor : IInterceptor
    {
        private readonly IRpcHttpClientProvider _httpClientProvider;
        private readonly INamingConventionService _namingConventionService;
        private readonly IHeaderProcessor[] _headerProcessorses;
        protected static int Id;

        public DynamicClientInterceptor(IRpcHttpClientProvider httpClientProvider, INamingConventionService namingConventionService, IHeaderProcessor[] headerProcessorses)
        {
            _httpClientProvider = httpClientProvider;
            _namingConventionService = namingConventionService;
            _headerProcessorses = headerProcessorses;
        }

        public void Intercept(IInvocation invocation)
        {
            var client = _httpClientProvider.GetHttpClient(invocation.TargetType);

            try
            {
                var task = ProcessCall(client, invocation);

                task.Wait(client.Timeout * 1000);
            }
            finally
            {
                _httpClientProvider.ReturnHttpClient(invocation.TargetType, client);
            }
        }

        protected virtual async Task ProcessCall(IRpcHttpClient client, IInvocation invocation)
        {
            var requestMessage = new RpcRequestMessage
            {
                Id = Interlocked.Increment(ref Id).ToString(),
                Method = _namingConventionService.GetMethodName(invocation.Method)
            };

            if (client.CallByParameterName)
            {
                requestMessage.Parameters = GetParameterByName(invocation);
            }
            else
            {
                requestMessage.Parameters = invocation.Arguments;
            }

            var httpRequest =
                new HttpRequestMessage(HttpMethod.Post, _namingConventionService.GetNameForType(invocation.Method.DeclaringType));

            httpRequest.Content =
                new StringContent(JsonConvert.SerializeObject(requestMessage), Encoding.UTF8, "application/json");

            foreach (var headerProcessorse in _headerProcessorses)
            {
                headerProcessorse.ProcessRequestHeader(httpRequest);
            }

            var response = await client.SendAsync(httpRequest);

            if (response.IsSuccessStatusCode)
            {
                foreach (var headerProcessorse in _headerProcessorses)
                {
                    headerProcessorse.ProcessResponseHeader(response);
                }

                var message =
                    JsonConvert.DeserializeObject<RpcResponseMessage>(await response.Content.ReadAsStringAsync());

                if (message.Result != null)
                {
                    invocation.ReturnValue = message.Result.ToObject(invocation.Method.ReturnType);
                }
                else
                {
                    if (message.Error != null)
                    {
                        if (message.Error.Code == (int) JsonRpcErrorCode.MethodNotFound)
                        {
                            throw new MethodNotFoundException(invocation.Method, invocation.Arguments);
                        }

                        if (message.Error.Code == (int)JsonRpcErrorCode.UnauthorizedAccess)
                        {
                            throw new UnauthorizedMethodException(invocation.Method, invocation.Arguments);
                        }

                        throw new InternalServerErrorException(invocation.Method, invocation.Arguments, message.Error.Message);
                    }

                    throw new DynamicMethodException(invocation.Method, invocation.Arguments,"Unknown error");
                }
            }
            else
            {
                if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    throw new UnauthorizedMethodException(invocation.Method, invocation.Arguments);
                }

                throw new DynamicMethodException(invocation.Method,invocation.Arguments, $"Error response status {response.StatusCode}");
            }
        }

        private object GetParameterByName(IInvocation invocation)
        {
            var parameters = invocation.Method.GetParameters();
            Dictionary<string, object> values = new Dictionary<string, object>();

            for (int i = 0; i < parameters.Length; i++)
            {
                values[parameters[i].Name] = invocation.Arguments[i];
            }

            return values;
        }
    }
}
