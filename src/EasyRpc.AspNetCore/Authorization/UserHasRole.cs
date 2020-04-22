using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EasyRpc.AspNetCore.Authorization
{
    /// <summary>
    /// User has role authorization
    /// </summary>
    public class UserHasRole : IEndPointMethodAuthorization
    {
        private string _role;

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="role"></param>
        public UserHasRole(string role)
        {
            _role = role;
        }

        /// <inheritdoc />
        public Task<bool> AsyncAuthorize(RequestExecutionContext context)
        {
            return Task.FromResult(context.HttpContext.User?.IsInRole(_role) ?? false);
        }
    }
}
