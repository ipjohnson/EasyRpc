using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using EasyRpc.AspNetCore.Authorization;
using EasyRpc.AspNetCore.Filters;
using EasyRpc.AspNetCore.CodeGeneration;
using EasyRpc.AspNetCore.ResponseHeader;
using EasyRpc.AspNetCore.Routing;

namespace EasyRpc.AspNetCore.EndPoints
{
    public delegate Task MethodEndPointDelegate(RequestExecutionContext context);

    public interface IMethodInvokeInformation
    {
        MethodInfo Signature { get; }

        /// <summary>
        /// Allows for custom method invoker to be provided
        /// </summary>
        MethodEndPointDelegate MethodInvokeDelegate { get;  }

        /// <summary>
        /// Method to invoke, will be null if DelegateToInvoke is provided
        /// </summary>
        MethodInfo MethodToInvoke { get; }

        /// <summary>
        /// Delegate to invoke, will be null if MethodToInvoke is provided
        /// </summary>
        Delegate DelegateToInvoke { get; }
    }

    /// <summary>
    /// Represents the method that needs to be invoked
    /// </summary>
    public class MethodInvokeInformation : IMethodInvokeInformation
    {
        /// <summary>
        /// Invoke signature
        /// </summary>
        public MethodInfo Signature => MethodToInvoke ?? MethodInvokeDelegate?.Method ?? MethodInvokeDelegate?.Method;
        
        /// <summary>
        /// Allows for custom method invoker to be provided
        /// </summary>
        public MethodEndPointDelegate MethodInvokeDelegate { get; set; }

        /// <summary>
        /// Method to invoke, will be null if DelegateToInvoke is provided
        /// </summary>
        public MethodInfo MethodToInvoke { get; set; }

        /// <summary>
        /// Delegate to invoke, will be null if MethodToInvoke is provided
        /// </summary>
        public Delegate DelegateToInvoke { get; set; }
    }

    /// <inheritdoc />
    public class EndPointMethodConfiguration : IEndPointMethodConfigurationReadOnly
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="routeInformation"></param>
        /// <param name="activationFunc"></param>
        /// <param name="invokeInformation"></param>
        /// <param name="returnType"></param>
        /// <param name="supportsCompression"></param>
        public EndPointMethodConfiguration(RpcRouteInformation routeInformation, 
            Func<RequestExecutionContext,object> activationFunc,
            MethodInvokeInformation invokeInformation, 
            Type returnType,
            bool? supportsCompression)
        {
            RouteInformation = routeInformation;
            ActivationFunc = activationFunc;
            SupportsCompression = supportsCompression;
            ReturnType = returnType;
            InvokeInformation = invokeInformation;
            Parameters = new List<RpcParameterInfo>();
        }

        /// <summary>
        /// Route information
        /// </summary>
        public RpcRouteInformation RouteInformation { get; }

        /// <inheritdoc />
        IRpcRouteInformation IEndPointMethodConfigurationReadOnly.RouteInformation => RouteInformation;

        /// <inheritdoc />
        public IReadOnlyList<Func<RequestExecutionContext,IRequestFilter>> Filters { get; set; }

        /// <inheritdoc />
        public IReadOnlyList<IEndPointMethodAuthorization> Authorizations { get; set; }

        /// <summary>
        /// Invoke information for method
        /// </summary>
        public MethodInvokeInformation InvokeInformation { get; }

        /// <inheritdoc />
        IMethodInvokeInformation IEndPointMethodConfigurationReadOnly.InvokeInformation => InvokeInformation;

        /// <inheritdoc />
        public Func<RequestExecutionContext, object> ActivationFunc { get; }

        /// <inheritdoc />
        public bool? SupportsCompression { get; }

        /// <summary>
        /// List of parameters for method
        /// </summary>
        public List<RpcParameterInfo> Parameters { get; }

        /// <inheritdoc />
        IReadOnlyList<RpcParameterInfo> IEndPointMethodConfigurationReadOnly.Parameters => Parameters;

        /// <inheritdoc />
        public Type ReturnType { get; }

        /// <inheritdoc />
        public int SuccessStatusCode { get; set; } = (int)HttpStatusCode.OK;

        /// <inheritdoc />
        public string RawContentType { get; set; }

        /// <inheritdoc />
        public string RawContentEncoding { get; set; }

        /// <inheritdoc />
        public IReadOnlyList<IResponseHeader> ResponseHeaders { get; set; }
    }
}
