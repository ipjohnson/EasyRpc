using System.Threading.Tasks;

namespace EasyRpc.AspNetCore.Middleware
{
    public interface IMethodAuthorization
    {
        Task<bool> AsyncAuthorize(ICallExecutionContext context);
    }
}
