using System;
using System.Collections.Generic;
using System.Text;
using EasyRpc.AspNetCore.EndPoints;

namespace EasyRpc.AspNetCore.ModelBinding
{
    public interface ISpecialParameterBinder
    {
        void BindRequestServicesParameter(RequestExecutionContext context,
            EndPointMethodConfiguration endPointMethodConfiguration, IRpcParameterInfo parameter,
            IRequestParameters parameterContext);

        void BindHttpContextParameter(RequestExecutionContext context,
            EndPointMethodConfiguration endPointMethodConfiguration, IRpcParameterInfo parameter,
            IRequestParameters parameterContext);

        void BindRequestExecutionContextParameter(RequestExecutionContext context,
            EndPointMethodConfiguration endPointMethodConfiguration, IRpcParameterInfo parameter,
            IRequestParameters parameterContext);

        void BindHttpRequestParameter(RequestExecutionContext context,
            EndPointMethodConfiguration endPointMethodConfiguration, IRpcParameterInfo parameter,
            IRequestParameters parameterContext);

        void BindHttpResponseParameter(RequestExecutionContext context,
            EndPointMethodConfiguration endPointMethodConfiguration, IRpcParameterInfo parameter,
            IRequestParameters parameterContext);

        void BindHttpCancellationTokenParameter(RequestExecutionContext context,
            EndPointMethodConfiguration endPointMethodConfiguration, IRpcParameterInfo parameter,
            IRequestParameters parameterContext);
    }

    public class SpecialParameterBinder : ISpecialParameterBinder
    {
        public virtual void BindRequestServicesParameter(RequestExecutionContext context,
            EndPointMethodConfiguration endPointMethodConfiguration, IRpcParameterInfo parameter,
            IRequestParameters parameterContext)
        {
            parameterContext[parameter.Position] = context.HttpContext.RequestServices.GetService(parameter.ParamType);
        }

        public virtual void BindHttpContextParameter(RequestExecutionContext context,
            EndPointMethodConfiguration endPointMethodConfiguration, IRpcParameterInfo parameter,
            IRequestParameters parameterContext)
        {
            parameterContext[parameter.Position] = context.HttpContext;
        }

        public virtual void BindRequestExecutionContextParameter(RequestExecutionContext context,
            EndPointMethodConfiguration endPointMethodConfiguration, IRpcParameterInfo parameter,
            IRequestParameters parameterContext)
        {
            parameterContext[parameter.Position] = context;
        }

        public virtual void BindHttpRequestParameter(RequestExecutionContext context,
            EndPointMethodConfiguration endPointMethodConfiguration, IRpcParameterInfo parameter,
            IRequestParameters parameterContext)
        {
            parameterContext[parameter.Position] = context.HttpContext.Request;
        }

        public virtual void BindHttpResponseParameter(RequestExecutionContext context,
            EndPointMethodConfiguration endPointMethodConfiguration, IRpcParameterInfo parameter,
            IRequestParameters parameterContext)
        {
            parameterContext[parameter.Position] = context.HttpContext.Response;
        }

        public virtual void BindHttpCancellationTokenParameter(RequestExecutionContext context,
            EndPointMethodConfiguration endPointMethodConfiguration, IRpcParameterInfo parameter,
            IRequestParameters parameterContext)
        {
            parameterContext[parameter.Position] = context.HttpContext.RequestAborted;
        }
    }
}
