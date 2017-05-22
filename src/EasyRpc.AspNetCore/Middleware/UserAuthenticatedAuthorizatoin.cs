using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace EasyRpc.AspNetCore.Middleware
{
    public class UserAuthenticatedAuthorization : IMethodAuthorization
    {
        public Task<bool> AsyncAuthorize(HttpContext context)
        {
            return Task.FromResult(context.User?.Identity?.IsAuthenticated ?? false);
        }
    }
}
