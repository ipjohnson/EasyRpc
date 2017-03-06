using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EasyRpc.DynamicClient
{
    public interface IProxyRpcClientGenerator
    {
        Type GenerateProxyClientType(Type proxyType);
    }

    public class ProxyRpcClientGenerator
    {

    }
}
