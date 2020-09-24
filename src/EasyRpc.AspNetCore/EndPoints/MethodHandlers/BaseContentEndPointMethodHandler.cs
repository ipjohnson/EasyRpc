using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using EasyRpc.AspNetCore.Errors;
using EasyRpc.AspNetCore.Filters;
using EasyRpc.AspNetCore.Routing;
using EasyRpc.AspNetCore.Serializers;
using Microsoft.AspNetCore.Http;


namespace EasyRpc.AspNetCore.EndPoints.MethodHandlers
{
    /// <summary>
    /// Base end point method handler
    /// </summary>
    public abstract class BaseContentEndPointMethodHandler<TReturn> : IEndPointMethodHandler
    {

        /// <summary>
        /// Method for binding parameters
        /// </summary>
        protected MethodEndPointDelegate BindParametersDelegate;

        /// <summary>
        /// Invoke method delegate
        /// </summary>
        protected InvokeMethodDelegate<TReturn> InvokeMethodDelegate;

        /// <summary>
        /// Response delegate
        /// </summary>
        protected MethodEndPointDelegate ResponseDelegate;

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="configuration"></param>
        /// <param name="services"></param>
        protected BaseContentEndPointMethodHandler(EndPointMethodConfiguration configuration, EndPointServices services)
        {
            Configuration = configuration;
            Services = services;
        }

        /// <inheritdoc />
        public EndPointServices Services { get; }

        /// <inheritdoc />
        public IRpcRouteInformation RouteInformation => Configuration.RouteInformation;

        /// <inheritdoc />
        public virtual IEndPointMethodConfigurationReadOnly Configuration { get; }

        /// <inheritdoc />
        public virtual string HttpMethod => RouteInformation.Method;

        /// <inheritdoc />
        public abstract Task HandleRequest(HttpContext context);

       /// <summary>
        /// Builds method delegates
        /// </summary>
        protected virtual void SetupMethod()
        {
            lock (this)
            {
                if (BindParametersDelegate == null)
                {
                    BindParametersDelegate =
                        Services.ParameterBinderDelegateBuilder.CreateParameterBindingMethod(Configuration, out var parametersType);

                    if (Configuration.InvokeInformation.MethodInvokeDelegate == null)
                    {
                        InvokeMethodDelegate =
                            Services.MethodInvokerCreationService.BuildMethodInvoker<TReturn>(Configuration, parametersType);
                    }
                    else
                    {
                        InvokeMethodDelegate = 
                            (InvokeMethodDelegate<TReturn>)(object)Configuration.InvokeInformation.MethodInvokeDelegate;
                    }

                    ResponseDelegate = Services.ResponseDelegateCreator.CreateResponseDelegate(Configuration);
                }
            }
        }
    }
}
