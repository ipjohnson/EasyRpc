using System;
using System.Collections.Generic;
using System.Linq;
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
        public StateBasedEndPointMethodHandler(EndPointMethodConfiguration configuration, BaseEndPointServices services)
            : base(configuration, services)
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
                _startingState = RequestState.BeforeExecuteTaskFilter;
            }
            else
            {
                _startingState = RequestState.ExecuteTask;
            }
        }

        /// <inheritdoc />
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

            var state = _startingState;

            return NextStep(ref state, ref requestContext);
        }

        private Task NextStep(ref RequestState state, ref RequestExecutionContext requestContext)
        {
            if (state == RequestState.Complete ||
                !requestContext.ContinueRequest)
            {
                return Task.CompletedTask;
            }

            switch (state)
            {
                case RequestState.CheckAuth:
                    return CheckAuthentication(RequestState.BindParameters, requestContext);

                case RequestState.BindParameters:
                    state = Configuration.Filters != null ? RequestState.BeforeExecuteTaskFilter : RequestState.ExecuteTask;

                    return BindParameters(ref state, ref requestContext);

                case RequestState.BeforeExecuteTaskFilter:
                    state = RequestState.ExecuteTask;

                    return ExecuteBeforeFilter(ref state, ref requestContext);

                case RequestState.ExecuteTask:
                    state = requestContext.CallFilters != null ? RequestState.AfterExecuteTaskFilter : RequestState.Response;

                    // only execute if no result was set in the filters
                    return requestContext.Result == null ? 
                        ExecuteTask(ref state, ref requestContext) : 
                        NextStep(ref state, ref requestContext);

                case RequestState.AfterExecuteTaskFilter:
                    state = RequestState.Response;

                    return ExecuteAfterFilter(ref state, ref requestContext);

                case RequestState.Response:
                    state = requestContext.CallFilters?.Any(f => f is IBaseRequestFinalizerFilter) ?? false
                        ? RequestState.FinalizeFilter
                        : RequestState.Complete;

                    return SendResponse(ref state, ref requestContext);

                case RequestState.FinalizeFilter:
                    state = RequestState.Complete;

                    return ExecuteFinalizerFilters(ref state, ref requestContext);

                default:
                    throw new Exception($"Unknown request state {state}");
            }
        }

        #region Request State
        public enum RequestState
        {
            CheckAuth,

            BindParameters,

            BeforeExecuteTaskFilter,

            ExecuteTask,

            AfterExecuteTaskFilter,

            Response,

            FinalizeFilter,

            Complete
        }
        #endregion

        #region Check Authentication
        private async Task CheckAuthentication(RequestState nextStep, RequestExecutionContext requestContext)
        {
            if (await Services.AuthorizationService.AuthorizeRequest(requestContext, Configuration.Authorizations))
            {
                await NextStep(ref nextStep, ref requestContext);
            }
            else
            {
                await Services.ErrorHandler.HandleUnauthorized(requestContext);
            }
        }
        #endregion

        #region Bind Parameters
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
        #endregion

        #region Before Execute Filters
        private Task ExecuteBeforeFilter(ref RequestState state, ref RequestExecutionContext requestContext)
        {
            try
            {
                var filterFuncList = Configuration.Filters;

                var filterList = new List<IRequestFilter> { Capacity = filterFuncList.Count };

                foreach (var filterFunc in filterFuncList)
                {
                    var filter = filterFunc(requestContext);

                    if (filter != null)
                    {
                        filterList.Add(filter);
                    }
                }

                if (filterFuncList.Count > 0)
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
                RequestState state,
                RequestExecutionContext requestContext,
                int index,
                IErrorHandler errorHandler)
            {
                try
                {
                    for (; index < requestContext.CallFilters.Count; index++)
                    {
                        if (requestContext.CallFilters[index] is IRequestExecutionFilter executionFilter)
                        {
                            executionFilter.BeforeExecute(requestContext);
                        }
                        else if (requestContext.CallFilters[index] is IAsyncRequestExecutionFilter asyncRequestExecution)
                        {
                            await asyncRequestExecution.BeforeExecuteAsync(requestContext);
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
        #endregion

        #region Execute Task
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
        #endregion

        #region After Execute Filter
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
                return Services.ErrorHandler.HandleException(requestContext, e);
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
                            await asyncRequestExecution.AfterExecuteAsync(requestContext);
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
        #endregion

        #region Send Response
        private Task SendResponse(ref RequestState state, ref RequestExecutionContext requestContext)
        {
            var response = ResponseDelegate(requestContext);

            if (response.IsCompletedSuccessfully)
            {
                return NextStep(ref state, ref requestContext);
            }

            return SendResponseAsync(this, state, requestContext, response, Services.ErrorHandler);

            static async Task SendResponseAsync(StateBasedEndPointMethodHandler<TReturn> stateBasedEndPointMethodHandler,
                RequestState state, RequestExecutionContext requestContext, Task response, IErrorHandler errorHandler)
            {
                try
                {
                    await response;

                    await stateBasedEndPointMethodHandler.NextStep(ref state, ref requestContext);
                }
                catch (Exception e)
                {
                    await errorHandler.HandleException(requestContext, e);
                }
            }
        }
        #endregion

        #region Finalize Filter
        private Task ExecuteFinalizerFilters(ref RequestState state, ref RequestExecutionContext requestContext)
        {
            try
            {
                for (var i = 0; i < requestContext.CallFilters.Count; i++)
                {
                    if (requestContext.CallFilters[i] is IRequestFinalizeFilter executionFilter)
                    {
                        executionFilter.Finalize(requestContext);
                    }
                    else if (requestContext.CallFilters[i] is IAsyncRequestFinalizeFilter)
                    {
                        return ExecuteFinalizerFilterAsync(this, state, requestContext, i, Services.ErrorHandler);
                    }
                }
            }
            catch (Exception e)
            {
                return Services.ErrorHandler.HandleException(requestContext, e);
            }

            return NextStep(ref state, ref requestContext);

            static async Task ExecuteFinalizerFilterAsync(StateBasedEndPointMethodHandler<TReturn> stateBasedEndPointMethodHandler,
                RequestState state, RequestExecutionContext requestContext, int i, IErrorHandler errorHandler)
            {
                try
                {
                    for (; i < requestContext.CallFilters.Count; i++)
                    {
                        if (requestContext.CallFilters[i] is IRequestFinalizeFilter executionFilter)
                        {
                            executionFilter.Finalize(requestContext);
                        }
                        else if (requestContext.CallFilters[i] is IAsyncRequestFinalizeFilter asyncRequestExecution)
                        {
                            await asyncRequestExecution.FinalizeAsync(requestContext);
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
        #endregion
    }
}
