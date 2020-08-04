using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using EasyRpc.DynamicClient.Serializers;

namespace EasyRpc.DynamicClient.ExecutionService
{
    public class RpcExecuteInformation
    {
        public IRpcHttpClientProvider ClientProvider { get; set; }

        public IClientSerializer Serializer { get; set; }

        public HttpMethod Method { get; set; }

        public bool CompressBody { get; set; }
    }


    public interface IRpcExecutionService
    {
        Task<T> ExecuteMethodWithValue<T>(RpcExecuteInformation executeInformation,
            string path,
            object bodyParameters,
            CancellationToken? cancellationToken);

        Task ExecuteMethod(RpcExecuteInformation executeInformation,
            string path,
            object bodyParameters,
            CancellationToken? cancellationToken);
    }
}
