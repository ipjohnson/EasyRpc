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
        /// <inheritdoc />
        public async Task<T> ExecuteMethodWithValue<T>(RpcExecuteInformation executeInformation, string path, object bodyParameters,
            CancellationToken? cancellationToken)
        {
            var request = new HttpRequestMessage(executeInformation.Method, path);

            if (bodyParameters != null)
            {
                await executeInformation.Serializer.SerializeToRequest(bodyParameters, request, executeInformation.CompressBody);
            }

            var client = executeInformation.ClientProvider.ProvideClient();

            using (var response = await client.SendAsync(request, cancellationToken ?? CancellationToken.None))
            {
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
            }

            throw new Exception("Need to handle error");
        }

        public async Task ExecuteMethod(RpcExecuteInformation executeInformation, string path, object bodyParameters,
            CancellationToken? cancellationToken)
        {
            var request = new HttpRequestMessage(executeInformation.Method, path);

            if (bodyParameters != null)
            {
                await executeInformation.Serializer.SerializeToRequest(bodyParameters, request, executeInformation.CompressBody);
            }

            var client = executeInformation.ClientProvider.ProvideClient();

            var response = await client.SendAsync(request, cancellationToken ?? CancellationToken.None);

            if (response.IsSuccessStatusCode)
            {
                return;
            }

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return;
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
    }
}
