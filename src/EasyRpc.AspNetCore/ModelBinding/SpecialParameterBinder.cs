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

        public virtual void BindHttpContextParameter(RequestExecutionContext context, RpcParameterInfo parameter, IRequestParameters parameterContext)
        {
            parameterContext[parameter.Position] = context.HttpContext;
        }

        public virtual void BindRequestExecutionContextParameter(RequestExecutionContext context, RpcParameterInfo parameter, IRequestParameters parameterContext)
        {
            parameterContext[parameter.Position] = context;
        }

        public virtual void BindHttpRequestParameter(RequestExecutionContext context, RpcParameterInfo parameter,
            IRequestParameters parameterContext)
        {
            parameterContext[parameter.Position] = context.HttpContext.Request;
        }

        public virtual void BindHttpResponseParameter(RequestExecutionContext context, RpcParameterInfo parameter, IRequestParameters parameterContext)
        {
            parameterContext[parameter.Position] = context.HttpContext.Response;
        }

        public virtual void BindHttpCancellationTokenParameter(RequestExecutionContext context, RpcParameterInfo parameter,
            IRequestParameters parameterContext)
        {
            parameterContext[parameter.Position] = context.HttpContext.RequestAborted;
        }
    }
}
