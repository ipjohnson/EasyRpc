using System;
using System.Collections.Generic;
using System.Text;
using EasyRpc.AspNetCore.Authorization;
using EasyRpc.AspNetCore.Data;
using EasyRpc.AspNetCore.Filters;
using EasyRpc.AspNetCore.ResponseHeader;

namespace EasyRpc.AspNetCore.Configuration.DelegateConfiguration
{
    public class ExpressionInstanceConfiguration : IExposureExpressionConfiguration
    {
        public string Path { get; set; }

        public string Method { get; set; }

        public bool HasBody { get; set; }

        public string RawContentType { get; set; }

        public ImmutableLinkedList<Func<RequestExecutionContext, IRequestFilter>> Filters { get; set; } = 
            ImmutableLinkedList<Func<RequestExecutionContext, IRequestFilter>>.Empty;

        public ImmutableLinkedList<IEndPointMethodAuthorization> Authorizations = ImmutableLinkedList<IEndPointMethodAuthorization>.Empty;
        
        public ImmutableLinkedList<IResponseHeader> ResponseHeaders = ImmutableLinkedList<IResponseHeader>.Empty;
        public IExposureExpressionConfiguration Authorize(string role = null, string policy = null)
        {
            throw new NotImplementedException();
        }

        public IExposureExpressionConfiguration Raw(string contentType)
        {
            RawContentType = contentType;

            return this;
        }
    }
}
