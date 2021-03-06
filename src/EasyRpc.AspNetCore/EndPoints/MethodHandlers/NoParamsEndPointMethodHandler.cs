﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using EasyRpc.AspNetCore.Errors;
using EasyRpc.AspNetCore.ModelBinding;
using Microsoft.AspNetCore.Http;

namespace EasyRpc.AspNetCore.EndPoints.MethodHandlers
{
    /// <summary>
    /// End point handler with no parameters, no authentication, no filters
    /// </summary>
    public class NoParamsEndPointMethodHandler<TReturn> : BaseContentEndPointMethodHandler<TReturn>
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="configuration"></param>
        /// <param name="services"></param>
        public NoParamsEndPointMethodHandler(EndPointMethodConfiguration configuration, EndPointServices services) : base(configuration, services)
        {

        }

        public override Task HandleRequest(HttpContext context)
        {
            var requestContext = new RequestExecutionContext(context, this, Configuration.SuccessStatusCode)
            {
                Parameters = EmptyParameters.Instance,
                CanCompress = Configuration.SupportsCompression.GetValueOrDefault(false)
            };
            
            if (ActivationFunc == null)
            {
                SetupMethod();
            }

            try
            {
                requestContext.ServiceInstance = ActivationFunc(requestContext);
            
                var executionTask = InvokeMethodDelegate(requestContext);
                
                if (!executionTask.IsCompletedSuccessfully)
                {
                    return AwaitInvoke(requestContext, executionTask, Services.ErrorHandler, ResponseDelegate);
                }

                requestContext.Result = executionTask.Result;

                if (context.Response.HasStarted)
                {
                    return executionTask;
                }

                var responseTask = ResponseDelegate(requestContext);

                return responseTask.IsCompletedSuccessfully ?
                    responseTask :
                    AwaitResponse(requestContext, responseTask, Services.ErrorHandler);
            }
            catch (Exception e)
            {
                return Services.ErrorHandler.HandleException(requestContext, e);
            }
        }

        private static async Task AwaitInvoke(RequestExecutionContext requestContext,
            Task<TReturn> executionResult,
            IErrorHandler servicesErrorHandler,
            MethodEndPointDelegate responseDelegate)
        {
            try
            {
                requestContext.Result = await executionResult;

                if (!requestContext.ResponseHasStarted)
                {
                    await responseDelegate(requestContext);
                }
            }
            catch (Exception e)
            {
                await servicesErrorHandler.HandleException(requestContext, e);
            }
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
    }
}
