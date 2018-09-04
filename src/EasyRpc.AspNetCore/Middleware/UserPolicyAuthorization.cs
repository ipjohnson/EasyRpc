using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;

namespace EasyRpc.AspNetCore.Middleware
{
    public class UserPolicyAuthorization : IMethodAuthorization
    {
        private readonly string _policy;

        public UserPolicyAuthorization(string policy)
        {
            _policy = policy;
        }

        public Task<bool> AsyncAuthorize(ICallExecutionContext context)
        {
            return context.HttpContext.RequestServices.GetRequiredService<IAuthorizationService>().
                AuthorizeAsync(context.HttpContext.User, _policy);
        }
    }
}
