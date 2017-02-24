using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
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

        public async Task<bool> AsyncAuthorize(HttpContext context)
        {
            var authorizationService = context.RequestServices.GetRequiredService<IAuthorizationService>();

            var returnvalue = await authorizationService.AuthorizeAsync(context.User, _policy);

            return returnvalue;
        }
    }
}
