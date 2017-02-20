using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace EasyRpc.AspNetCore.Middleware
{
    public class UserRoleAuthorization : IMethodAuthorization
    {
        private string _role;

        public UserRoleAuthorization(string role)
        {
            _role = role;
        }

        public Task<bool> AsyncAuthorize(HttpContext context)
        {
            return Task.FromResult(context.User?.IsInRole(_role) ?? false);
        }
    }
}
