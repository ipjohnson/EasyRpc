using System;
using System.Collections.Generic;
using System.Text;
using EasyRpc.AspNetCore.Authorization;

namespace EasyRpc.AspNetCore.Configuration
{
    public interface IAuthorizationImplementationProvider
    {
        IEndPointMethodAuthorization Authorized();
        IEndPointMethodAuthorization UserHasPolicy(string policy);
        IEndPointMethodAuthorization UserHasRole(string role);
    }

    public class AuthorizationImplementationProvider : IAuthorizationImplementationProvider
    {
        public virtual IEndPointMethodAuthorization Authorized()
        {
            return new UserIsAuthorized();
        }

        public virtual IEndPointMethodAuthorization UserHasPolicy(string policy)
        {
            return  new UserHasPolicy(policy);
        }

        public virtual IEndPointMethodAuthorization UserHasRole(string role)
        {
            return new UserHasRole(role);
        }
    }
}
