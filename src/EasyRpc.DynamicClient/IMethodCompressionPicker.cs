using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace EasyRpc.DynamicClient
{
    public interface IMethodCompressionPicker
    {
        bool CompressResponse(MethodInfo method);

        bool CompressRequest(MethodInfo method);
    }
}
