using System.Reflection;

namespace EasyRpc.DynamicClient
{
    public interface IMethodCompressionPicker
    {
        bool CompressResponse(MethodInfo method);

        bool CompressRequest(MethodInfo method);
    }
}
