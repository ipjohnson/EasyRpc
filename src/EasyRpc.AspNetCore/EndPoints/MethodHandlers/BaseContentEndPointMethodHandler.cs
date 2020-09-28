using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using EasyRpc.AspNetCore.Errors;
using EasyRpc.AspNetCore.Features;
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
        /// instance activation function
        /// </summary>
        protected Func<RequestExecutionContext, object> ActivationFunc;

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="configuration"></param>
        /// <param name="services"></param>
        protected BaseContentEndPointMethodHandler(EndPointMethodConfiguration configuration, EndPointServices services)
        {
            Configuration = configuration;
            Services = services;
            HttpMethod = configuration.RouteInformation.Method;
        }

        /// <inheritdoc />
        public EndPointServices Services { get; }

        /// <inheritdoc />
        public IRpcRouteInformation RouteInformation => Configuration.RouteInformation;

        /// <inheritdoc />
        public virtual IEndPointMethodConfigurationReadOnly Configuration { get; }

        /// <inheritdoc />
        public string HttpMethod {get; }

        /// <inheritdoc />
        public abstract Task HandleRequest(HttpContext context);

       /// <summary>
        /// Builds method delegates
        /// </summary>
        protected virtual void SetupMethod()
        {
            lock (this)
            {
                if (ActivationFunc == null)
                {
                    AssignActivationFunc();

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

       private void AssignActivationFunc()
       {
           var requestExecutionContextFeature = Services.ConfigurationManager
               .GetConfiguration<RequestExecutionContextFeatureConfiguration>().FeatureEnabled;

           if (requestExecutionContextFeature)
           {
               var activationFunc = Configuration.ActivationFunc;

                ActivationFunc = context =>
               {
                   context.HttpContext.Features.Set<IRequestExecutionContextFeature>(new RequestExecutionContextFeature(context));

                   return activationFunc(context);
               };
           }
           else
           {
               ActivationFunc = Configuration.ActivationFunc;
           }
       }
    }
}
