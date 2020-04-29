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
    public abstract class BaseContentEndPointMethodHandler : IEndPointMethodHandler
    {
        private object[] _serializerData = Array.Empty<object>();

        protected readonly BaseEndPointServices Services;
        protected MethodEndPointDelegate BindParametersDelegate;
        protected MethodEndPointDelegate InvokeMethodDelegate;
        protected MethodEndPointDelegate ResponseDelegate;

        protected BaseContentEndPointMethodHandler(EndPointMethodConfiguration configuration, BaseEndPointServices services)
        {
            Configuration = configuration;
            Services = services;
        }
        
        public IRpcRouteInformation RouteInformation => Configuration.RouteInformation;

        public virtual EndPointMethodConfiguration Configuration { get; }

        public virtual string HttpMethod => RouteInformation.Method;

        public abstract Task HandleRequest(HttpContext context);

        public object GetSerializerData(int serializerId)
        {
            return _serializerData.Length > serializerId ? _serializerData[serializerId] : null;
        }

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
