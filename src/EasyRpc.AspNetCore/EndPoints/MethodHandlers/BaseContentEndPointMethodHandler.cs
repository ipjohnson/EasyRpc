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
    public abstract class BaseContentEndPointMethodHandler : IEndPointMethodHandler
    {
        private object[] _serializerData = Array.Empty<object>();

        /// <summary>
        /// Services
        /// </summary>
        protected readonly BaseEndPointServices Services;

        /// <summary>
        /// Method for binding parameters
        /// </summary>
        protected MethodEndPointDelegate BindParametersDelegate;

        /// <summary>
        /// Invoke method delegate
        /// </summary>
        protected MethodEndPointDelegate InvokeMethodDelegate;

        /// <summary>
        /// Response delegate
        /// </summary>
        protected MethodEndPointDelegate ResponseDelegate;

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="configuration"></param>
        /// <param name="services"></param>
        protected BaseContentEndPointMethodHandler(EndPointMethodConfiguration configuration, BaseEndPointServices services)
        {
            Configuration = configuration;
            Services = services;
        }

        /// <inheritdoc />
        public IRpcRouteInformation RouteInformation => Configuration.RouteInformation;

        /// <inheritdoc />
        public virtual IEndPointMethodConfigurationReadOnly Configuration { get; }

        /// <inheritdoc />
        public virtual string HttpMethod => RouteInformation.Method;

        /// <inheritdoc />
        public abstract Task HandleRequest(HttpContext context);

        /// <inheritdoc />
        public object GetSerializerData(int serializerId)
        {
            return _serializerData.Length > serializerId ? _serializerData[serializerId] : null;
        }

        /// <inheritdoc />
        public void SetSerializerData(int serializerId, object data)
        {
            if (_serializerData.Length <= serializerId)
            {
                lock (this)
                {
                    var newData = new object[serializerId + 1];

                    Array.Copy(_serializerData, newData, _serializerData.Length);

                    _serializerData = newData;
                }
            }

            _serializerData[serializerId] = data;
        }

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
                            Services.MethodInvokerCreationService.BuildMethodInvoker(Configuration, parametersType);
                    }
                    else
                    {
                        InvokeMethodDelegate = Configuration.InvokeInformation.MethodInvokeDelegate;
                    }

                    ResponseDelegate = Services.ResponseDelegateCreator.CreateResponseDelegate(Configuration);
                }
            }
        }
    }
}
