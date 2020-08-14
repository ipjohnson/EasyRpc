using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;

namespace EasyRpc.AspNetCore.Authorization
{
    /// <summary>
    /// End point method authorization
    /// </summary>
    public class UserHasPolicy : IEndPointMethodAuthorization
    {
        private readonly string _policy;

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="policy"></param>
        public UserHasPolicy(string policy)
        {
            _policy = policy;
        }

        /// <inheritdoc />
        public async Task<bool> AsyncAuthorize(RequestExecutionContext context)
        {
            var authService = context.HttpContext.RequestServices.GetRequiredService<IAuthorizationService>();

            var result = await authService.AuthorizeAsync(context.HttpContext.User, _policy);

            return result.Succeeded;
        }
    }
}
