using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using EasyRpc.AspNetCore.Errors;
using EasyRpc.AspNetCore.ModelBinding;
using Microsoft.AspNetCore.Http;

namespace EasyRpc.AspNetCore.EndPoints.MethodHandlers
{
    /// <summary>
    /// End point handler with no parameters and no authentication
    /// </summary>
    public class NoParamsEndPointMethodHandler<TReturn> : BaseContentEndPointMethodHandler<TReturn>
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="configuration"></param>
        /// <param name="services"></param>
        public NoParamsEndPointMethodHandler(EndPointMethodConfiguration configuration, BaseEndPointServices services) : base(configuration, services)
        {

        }

        public override Task HandleRequest(HttpContext context)
        {
            var requestContext = new RequestExecutionContext(context, this, Configuration.SuccessStatusCode)
            {
                Parameters = EmptyParameters.Instance
            };

            if (InvokeMethodDelegate == null)
            {
                SetupMethod();
            }

            try
            {
                requestContext.ServiceInstance = Configuration.ActivationFunc(requestContext);
            }
            catch (Exception e)
            {
                return Services.ErrorHandler.HandleException(requestContext, e);
            }

            var executionResult = InvokeMethodDelegate(requestContext);

            if (!executionResult.IsCompletedSuccessfully)
            {
                return AwaitInvoke(requestContext, executionResult, Services.ErrorHandler, ResponseDelegate);
            }

            requestContext.Result = executionResult.Result;

            var responseResult = ResponseDelegate(requestContext);

            if (!responseResult.IsCompletedSuccessfully)
            {
                return AwaitResponse(requestContext, responseResult, Services.ErrorHandler);
            }

            return Task.CompletedTask;
        }

        private async Task AwaitResponse(RequestExecutionContext requestContext, Task responseResult, IErrorHandler servicesErrorHandler)
        {
            try
            {
                await responseResult;
            }
            catch (Exception e)
            {
                await servicesErrorHandler.HandleException(requestContext, e);
            }
        }

        private static async Task AwaitInvoke(RequestExecutionContext requestContext, Task<TReturn> executionResult,
            IErrorHandler servicesErrorHandler, MethodEndPointDelegate responseDelegate)
        {
            Exception exception = null;

            try
            {
                requestContext.Result = await executionResult;

                if (!requestContext.ResponseHasStarted &&
                    requestContext.ContinueRequest)
                {
                    var responseResult = responseDelegate(requestContext);

                    if (!responseResult.IsCompletedSuccessfully)
                    {
                        await responseResult;
                    }
                }
            }
            catch (Exception e)
            {
                exception = e;
            }

            if (exception != null)
            {
                await servicesErrorHandler.HandleException(requestContext, exception);
            }
        }
    }
}
