using System;
using System.Reflection;

namespace EasyRpc.DynamicClient.Grace.Impl
{
    public class DefaultMethodCompressionPicker : IMethodCompressionPicker
    {
        private readonly Func<MethodInfo, bool> _compressRequest;
        private readonly Func<MethodInfo, bool> _compressResponse;

        public DefaultMethodCompressionPicker(Func<MethodInfo, bool> compressRequest, Func<MethodInfo, bool> compressResponse)
        {
            _compressRequest = compressRequest ?? (method => false);
            _compressResponse = compressResponse ?? (method => false);
        }

        public bool CompressRequest(MethodInfo method)
        {
            return _compressRequest(method);
        }

        public bool CompressResponse(MethodInfo method)
        {
            return _compressResponse(method);
        }
    }
}
