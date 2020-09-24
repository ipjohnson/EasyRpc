using System;
using System.Collections.Generic;
using System.Text;
using EasyRpc.AspNetCore.Authorization;
using EasyRpc.AspNetCore.EndPoints;
using EasyRpc.AspNetCore.Filters;
using EasyRpc.AspNetCore.ResponseHeader;
using EasyRpc.AspNetCore.Routing;

namespace EasyRpc.AspNetCore
{
    /// <summary>
    /// EndPoint method configuration 
    /// </summary>
    public interface IEndPointMethodConfigurationReadOnly
    {
        /// <summary>
        /// End point route information
        /// </summary>
        IRpcRouteInformation RouteInformation { get; }

        /// <summary>
        /// List of filters, can be null
        /// </summary>
        IReadOnlyList<Func<RequestExecutionContext, IRequestFilter>> Filters { get; }

        /// <summary>
        /// List of authorizations, can be null
        /// </summary>
        IReadOnlyList<IEndPointMethodAuthorization> Authorizations { get; }

        /// <summary>
        /// Information about how end point should be invoked
        /// </summary>
        IMethodInvokeInformation InvokeInformation { get; }

        /// <summary>
        /// Func used to activate instance
        /// </summary>
        Func<RequestExecutionContext, object> ActivationFunc { get; }

        /// <summary>
        /// Should result support compression
        /// </summary>
        bool? SupportsCompression { get; }

        /// <summary>
        /// List of parameters 
        /// </summary>
        IReadOnlyList<RpcParameterInfo> Parameters { get; }

        /// <summary>
        /// Return type of method
        /// </summary>
        Type ReturnType { get; }

        /// <summary>
        /// Has response body
        /// </summary>
        bool HasResponseBody { get; }

        /// <summary>
        /// 
        /// </summary>
        Type WrappedType { get; }

        /// <summary>
        /// Successful http status code (200)
        /// </summary>
        int SuccessStatusCode { get; }

        /// <summary>
        /// If set this call will return raw bytes on the response body with the specified content type
        /// </summary>
        string RawContentType { get; }

        /// <summary>
        /// Sets content encoding header if set
        /// </summary>
        string RawContentEncoding { get; }

        /// <summary>
        /// List of response headers to be applied to end point
        /// </summary>
        IReadOnlyList<IResponseHeader> ResponseHeaders { get; }
    }
}
