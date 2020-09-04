using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using EasyRpc.AspNetCore.Errors;
using EasyRpc.AspNetCore.Filters;
using EasyRpc.AspNetCore.ModelBinding;
using Microsoft.AspNetCore.Http;

namespace EasyRpc.AspNetCore.EndPoints.MethodHandlers
{
    public class StateBasedEndPointMethodHandler<TReturn> : BaseContentEndPointMethodHandler<TReturn>
    {
        private readonly RequestState _startingState;

        /// <inheritdoc />
        public StateBasedEndPointMethodHandler(EndPointMethodConfiguration configuration, BaseEndPointServices services) : base(configuration, services)
        {
            if (configuration.Authorizations != null && configuration.Authorizations.Count > 0)
            {
                _startingState = RequestState.CheckAuth;
            }
            else if (configuration.Parameters.Count > 0)
            {
                _startingState = RequestState.BindParameters;
            }
            else if (configuration.Filters != null && configuration.Filters.Count > 0)
            {
                _startingState = RequestState.BeforeFilter;
            }
            else
            {
                _startingState = RequestState.ExecuteTask;
            }
        }


        /// <inheritdoc />
        public override Task HandleRequest(HttpContext context)
        {
            var requestContext = new RequestExecutionContext(context, this, Configuration.SuccessStatusCode);

            if (InvokeMethodDelegate == null)
            {
                SetupMethod();
            }

            try
            {
                requestContext.Parameters = EmptyParameters.Instance;

                requestContext.ServiceInstance = Configuration.ActivationFunc(requestContext);
            }
            catch (Exception e)
            {
                return Services.ErrorHandler.HandleException(requestContext, e);
            }

            var state = _startingState;

            return NextStep(ref state, ref requestContext);
        }


        private Task NextStep(ref RequestState state, ref RequestExecutionContext requestContext)
        {
            if (requestContext.ContinueRequest)
            {
                switch (state)
                {
                    case RequestState.CheckAuth:
                        return CheckAuthentication(RequestState.BindParameters, requestContext);

                    case RequestState.BindParameters:
                        state = Configuration.Filters != null ? RequestState.BeforeFilter : RequestState.ExecuteTask;

                        return BindParameters(ref state, ref requestContext);

                    case RequestState.BeforeFilter:
                        state = RequestState.ExecuteTask;

                        return ExecuteBeforeFilter(ref state, ref requestContext);

                    case RequestState.ExecuteTask:
                        state = requestContext.CallFilters != null ? RequestState.AfterFilter : RequestState.Response;

                        return ExecuteTask(ref state, ref requestContext);

                    case RequestState.AfterFilter:
                        state = RequestState.Response;

                        return ExecuteAfterFilter(ref state, ref requestContext);

                    case RequestState.Response:
                        return SendResponse(ref requestContext);
                }
            }
            
            return Task.CompletedTask;
        }

        private Task ExecuteBeforeFilter(ref RequestState state, ref RequestExecutionContext requestContext)
        {
            try
            {
                var filters = Configuration.Filters;

                var filterList = new List<IRequestFilter> { Capacity = filters.Count };

                foreach (var filterFunc in filters)
                {
                    var filter = filterFunc(requestContext);

                    if (filter != null)
                    {
                        filterList.Add(filter);
                    }
                }

                if (filters.Count > 0)
                {
                    requestContext.CallFilters = filterList;

                    for (var i = 0; i < requestContext.CallFilters.Count; i++)
                    {
                        if (requestContext.CallFilters[i] is IRequestExecutionFilter executionFilter)
                        {
                            executionFilter.BeforeExecute(requestContext);
                        }
                        else if (requestContext.CallFilters[i] is IAsyncRequestExecutionFilter)
                        {
                            return ExecuteBeforeFilterAsync(this, state, requestContext, i, Services.ErrorHandler);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                return Services.ErrorHandler.HandleException(requestContext, e);
            }

            return NextStep(ref state, ref requestContext);

            static async Task ExecuteBeforeFilterAsync(StateBasedEndPointMethodHandler<TReturn> stateBasedEndPointMethodHandler,
                RequestState state, RequestExecutionContext requestContext, int i, IErrorHandler errorHandler)
            {
                try
                {
                    for (; i < requestContext.CallFilters.Count; i++)
                    {
                        if (requestContext.CallFilters[i] is IRequestExecutionFilter executionFilter)
                        {
                            executionFilter.BeforeExecute(requestContext);
                        }
                        else if (requestContext.CallFilters[i] is IAsyncRequestExecutionFilter asyncRequestExecution)
                        {
                            await asyncRequestExecution.BeforeExecute(requestContext);
                        }
                    }

                    await stateBasedEndPointMethodHandler.NextStep(ref state, ref requestContext);
                }
                catch (Exception e)
                {
                    await errorHandler.HandleException(requestContext, e);
                }
            }
        }

        private Task ExecuteAfterFilter(ref RequestState state, ref RequestExecutionContext requestContext)
        {
            try
            {
                for (var i = 0; i < requestContext.CallFilters.Count; i++)
                {
                    if (requestContext.CallFilters[i] is IRequestExecutionFilter executionFilter)
                    {
                        executionFilter.AfterExecute(requestContext);
                    }
                    else if (requestContext.CallFilters[i] is IAsyncRequestExecutionFilter)
                    {
                        return ExecuteAfterFilterAsync(this, state, requestContext, i, Services.ErrorHandler);
                    }
                }
            }
            catch (Exception e)
            {
                Services.ErrorHandler.HandleException(requestContext, e);
            }

            return NextStep(ref state, ref requestContext);

            static async Task ExecuteAfterFilterAsync(StateBasedEndPointMethodHandler<TReturn> stateBasedEndPointMethodHandler,
                RequestState state, RequestExecutionContext requestContext, int i, IErrorHandler errorHandler)
            {
                try
                {
                    for (; i < requestContext.CallFilters.Count; i++)
                    {
                        if (requestContext.CallFilters[i] is IRequestExecutionFilter executionFilter)
                        {
                            executionFilter.AfterExecute(requestContext);
                        }
                        else if (requestContext.CallFilters[i] is IAsyncRequestExecutionFilter asyncRequestExecution)
                        {
                            await asyncRequestExecution.AfterExecute(requestContext);
                        }
                    }

                    await stateBasedEndPointMethodHandler.NextStep(ref state, ref requestContext);
                }
                catch (Exception e)
                {
                    await errorHandler.HandleException(requestContext, e);
                }
            }
        }

        private Task SendResponse(ref RequestExecutionContext requestContext)
        {
            var response = ResponseDelegate(requestContext);

            if (response.IsCompletedSuccessfully)
            {
                return Task.CompletedTask;
            }

            return SendResponseAsync(requestContext, response, Services.ErrorHandler);
            
            static async Task SendResponseAsync(RequestExecutionContext requestContext, Task response, IErrorHandler errorHandler)
            {
                try
                {
                    await response;
                }
                catch (Exception e)
                {
                    await errorHandler.HandleException(requestContext, e);
                }
            }
        }

        private Task ExecuteTask(ref RequestState state, ref RequestExecutionContext requestContext)
        {
            var taskResult = InvokeMethodDelegate(requestContext);

            if (taskResult.IsCompletedSuccessfully)
            {
                requestContext.Result = taskResult.Result;

                return NextStep(ref state, ref requestContext);
            }

            return ExecuteTaskAsync(this, state, requestContext, taskResult, Services.ErrorHandler);

            static async Task ExecuteTaskAsync(StateBasedEndPointMethodHandler<TReturn> stateBasedEndPointMethodHandler,
                RequestState state, RequestExecutionContext requestContext, Task<TReturn> response, IErrorHandler errorHandler)
            {
                try
                {
                    requestContext.Result = await response;

                    await stateBasedEndPointMethodHandler.NextStep(ref state, ref requestContext);
                }
                catch (Exception e)
                {
                    await errorHandler.HandleException(requestContext, e);
                }
            }
        }

        private Task BindParameters(ref RequestState state, ref RequestExecutionContext requestContext)
        {
            var bindResult = BindParametersDelegate(requestContext);

            if (bindResult.IsCompletedSuccessfully)
            {
                return NextStep(ref state, ref requestContext);
            }

            return AsyncBindParameter(this, state, requestContext, bindResult, Services.ErrorHandler);

            static async Task AsyncBindParameter(StateBasedEndPointMethodHandler<TReturn> handler, RequestState state,
                RequestExecutionContext requestContext, Task result, IErrorHandler errorHandler)
            {
                try
                {
                    await result;

                    await handler.NextStep(ref state, ref requestContext);
                }
                catch (Exception e)
                {
                    await errorHandler.HandleException(requestContext, e);
                }
            }
        }

        private async Task CheckAuthentication(RequestState nextStep, RequestExecutionContext requestContext)
        {
            if (!await Services.AuthorizationService.AuthorizeRequest(requestContext, Configuration.Authorizations))
            {
                await Services.ErrorHandler.HandleUnauthorized(requestContext);
            }
            else
            {
                await NextStep(ref nextStep, ref requestContext);
            }
        }

        public enum RequestState
        {
            CheckAuth,

            BindParameters,

            BeforeFilter,

            ExecuteTask,

            AfterFilter,

            Response
        }
    }
}
