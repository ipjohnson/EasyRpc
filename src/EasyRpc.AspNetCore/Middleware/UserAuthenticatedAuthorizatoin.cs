using System.Threading.Tasks;

namespace EasyRpc.AspNetCore.Middleware
{
    public class UserAuthenticatedAuthorization : IMethodAuthorization
    {
        public Task<bool> AsyncAuthorize(ICallExecutionContext context)
        {
            return Task.FromResult(context.HttpContext.User?.Identity?.IsAuthenticated ?? false);
        }
    }
}
