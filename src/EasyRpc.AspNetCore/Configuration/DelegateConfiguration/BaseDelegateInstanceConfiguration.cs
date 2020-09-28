using System;
using System.Collections.Generic;
using System.Text;
using EasyRpc.AspNetCore.Authorization;
using EasyRpc.AspNetCore.Data;
using EasyRpc.AspNetCore.Filters;
using EasyRpc.AspNetCore.ResponseHeader;

namespace EasyRpc.AspNetCore.Configuration.DelegateConfiguration
{
    public abstract class BaseDelegateInstanceConfiguration : IExposureExpressionConfiguration, IConfigurationInformationProvider
    {
        public string Path { get; set; }

        public string Method { get; set; }

        public bool HasBody { get; set; }

        public string RawContentType { get; set; }

        public ImmutableLinkedList<IResponseHeader> Headers { get; set; } = ImmutableLinkedList<IResponseHeader>.Empty;

        public ImmutableLinkedList<Func<RequestExecutionContext, IRequestFilter>> Filters { get; set; } = 
            ImmutableLinkedList<Func<RequestExecutionContext, IRequestFilter>>.Empty;

        public ImmutableLinkedList<IEndPointMethodAuthorization> Authorizations = ImmutableLinkedList<IEndPointMethodAuthorization>.Empty;
        
        public IExposureExpressionConfiguration Authorize(string role = null, string policy = null)
        {
            IEndPointMethodAuthorization authorization;

            if (!string.IsNullOrEmpty(policy))
            {
                authorization = new UserHasPolicy(policy);
            }
            else if (!string.IsNullOrEmpty(role))
            {
                authorization = new UserHasRole(role);
            }
            else
            {
                authorization = new UserIsAuthorized();
            }

            Authorizations = Authorizations.Add(authorization);

            return this;
        }

        public IExposureExpressionConfiguration Raw(string contentType)
        {
            RawContentType = contentType;

            return this;
        }

        public IExposureExpressionConfiguration Header(string header, string value)
        {
            Headers = Headers.Add(new ResponseHeader.ResponseHeader(header, value));

            return this;
        }

        public abstract void ExecuteConfiguration(IApplicationConfigurationService service);
    }
}
