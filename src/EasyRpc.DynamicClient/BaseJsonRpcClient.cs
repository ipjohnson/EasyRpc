using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EasyRpc.DynamicClient
{
    public abstract class BaseJsonRpcClient
    {
        private IHeaderProcessor _headerProcessors;
        private IRpcHttpClientProvider _clientProvider;

        protected BaseJsonRpcClient(IHeaderProcessor headerProcessors, IRpcHttpClientProvider clientProvider)
        {
            _headerProcessors = headerProcessors;
            _clientProvider = clientProvider;
        }

        protected T MakeCall<T>(string className, string methodName, byte[] request)
        {


            return default(T);
        }
    }
}
