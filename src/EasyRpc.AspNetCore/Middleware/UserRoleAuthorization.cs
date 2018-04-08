using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace EasyRpc.AspNetCore.Middleware
{
    public class UserRoleAuthorization : IMethodAuthorization
    {
        private readonly string _role;

        public UserRoleAuthorization(string role)
        {
            _role = role;
        }

        public Task<bool> AsyncAuthorize(ICallExecutionContext context)
        {
            return Task.FromResult(context.HttpContext.User?.IsInRole(_role) ?? false);
        }
    }
}
