using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Http;

namespace EasyRpc.AspNetCore.ResponseHeader
{
    /// <summary>
    /// Interface to apply headers on response
    /// </summary>
    public interface IResponseHeader
    {
        /// <summary>
        /// Apply header to HttpResponse 
        /// </summary>
        /// <param name="context">request context</param>
        /// <param name="headers">response headers</param>
        void ApplyHeader(RequestExecutionContext context, IHeaderDictionary headers);
    }
}
