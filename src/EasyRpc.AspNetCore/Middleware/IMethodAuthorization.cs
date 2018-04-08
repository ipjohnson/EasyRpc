using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace EasyRpc.AspNetCore.Middleware
{
    public interface IMethodAuthorization
    {
        Task<bool> AsyncAuthorize(ICallExecutionContext context);
    }
}
