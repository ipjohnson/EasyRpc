using System;
using System.Collections.Generic;
using System.Text;

namespace EasyRpc.AspNetCore.ModelBinding
{
    public interface ISpecialParameterBinder
    {
        void BindRequestServicesParameter(RequestExecutionContext context, RpcParameterInfo parameter, IRequestParameters parameterContext);

        void BindHttpContextParameter(RequestExecutionContext context, RpcParameterInfo parameter, IRequestParameters parameterContext);

        void BindRequestExecutionContextParameter(RequestExecutionContext context, RpcParameterInfo parameter, IRequestParameters parameterContext);

        void BindHttpRequestParameter(RequestExecutionContext context, RpcParameterInfo parameter,
            IRequestParameters parameterContext);

        void BindHttpResponseParameter(RequestExecutionContext context, RpcParameterInfo parameter, IRequestParameters parameterContext);

        void BindHttpCancellationTokenParameter(RequestExecutionContext context, RpcParameterInfo parameter, IRequestParameters parameterContext);
    }

    public class SpecialParameterBinder : ISpecialParameterBinder
    {
        public virtual void BindRequestServicesParameter(RequestExecutionContext context, RpcParameterInfo parameter, IRequestParameters parameterContext)
        {
            parameterContext[parameter.Position] = context.HttpContext.RequestServices.GetService(parameter.ParamType);
        }

        public void BindHttpContextParameter(RequestExecutionContext context, RpcParameterInfo parameter, IRequestParameters parameterContext)
        {
            parameterContext[parameter.Position] = context.HttpContext;
        }

        public void BindRequestExecutionContextParameter(RequestExecutionContext context, RpcParameterInfo parameter, IRequestParameters parameterContext)
        {
            parameterContext[parameter.Position] = context;
        }

        public void BindHttpRequestParameter(RequestExecutionContext context, RpcParameterInfo parameter,
            IRequestParameters parameterContext)
        {
            parameterContext[parameter.Position] = context.HttpContext.Request;
        }

        public void BindHttpResponseParameter(RequestExecutionContext context, RpcParameterInfo parameter, IRequestParameters parameterContext)
        {
            parameterContext[parameter.Position] = context.HttpContext.Response;
        }

        public void BindHttpCancellationTokenParameter(RequestExecutionContext context, RpcParameterInfo parameter,
            IRequestParameters parameterContext)
        {
            parameterContext[parameter.Position] = context.HttpContext.RequestAborted;
        }
    }
}
