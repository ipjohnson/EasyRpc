using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;

namespace EasyRpc.AspNetCore.Authorization
{
    public class UserHasPolicy :IEndPointMethodAuthorization
    {
        private string _policy;

        public UserHasPolicy(string policy)
        {
            _policy = policy;
        }

        public async Task<bool> AsyncAuthorize(RequestExecutionContext context)
        {
            var authService = context.HttpContext.RequestServices.GetRequiredService<IAuthorizationService>();

            var result = await authService.AuthorizeAsync(context.HttpContext.User, _policy);

            return result.Succeeded;
        }
    }
}
