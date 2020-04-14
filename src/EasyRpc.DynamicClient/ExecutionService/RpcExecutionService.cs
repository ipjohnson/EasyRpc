using System;
using System.Collections.Generic;
using System.Drawing;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EasyRpc.DynamicClient.ExecutionService
{
    public class RpcExecutionService : IRpcExecutionService
    {
        private readonly IRpcHttpClientProvider _clientProvider;

        public RpcExecutionService(IRpcHttpClientProvider clientProvider)
        {
            _clientProvider = clientProvider;
        }

        public async Task<T> ExecuteMethod<T>(RpcExecuteInformation executeInformation, 
            string path, 
            object bodyParameters,
            CancellationToken? cancellationToken)
        {
            var request = new HttpRequestMessage(executeInformation.Method, path);

            if (bodyParameters != null)
            {
                executeInformation.Serializer.SerializeToRequest(bodyParameters, request, executeInformation.CompressBody);
            }

            var client = _clientProvider.ProvideClient(executeInformation.HostKey);

            var response = await client.SendAsync(request, cancellationToken ?? CancellationToken.None);

            if (response.IsSuccessStatusCode)
            {
                if (response.StatusCode == HttpStatusCode.NoContent)
                {
                    return default;
                }

                return await executeInformation.Serializer.DeserializeFromResponse<T>(response);
            }

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return default;
            }

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                throw new UnauthorizedAccessException("User is not currently authorized");
            }

            if (response.StatusCode == HttpStatusCode.Forbidden)
            {
                throw new UnauthorizedAccessException("User is forbidden from executing method");
            }

            throw new Exception("Need to handle error");
        }

        public Task ExecuteMethod(RpcExecuteInformation executeInformation, string path, object bodyParameters,
            CancellationToken? cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
