using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using EasyRpc.AspNetCore.EndPoints;
using EasyRpc.AspNetCore.Filters;

namespace EasyRpc.AspNetCore.CodeGeneration
{
    public static class InvokeHelpers
    {
        public static async Task ApplyFiltersAndInvoke(RequestExecutionContext requestContext, IReadOnlyList<Func<RequestExecutionContext, IRequestFilter>> filters, MethodEndPointDelegate invoke)
        {
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

                foreach (var filter in filterList)
                {
                    if (filter is IAsyncRequestExecutionFilter asyncFilter)
                    {
                        await asyncFilter.BeforeExecute(requestContext);
                    }
                    else if (filter is IRequestExecutionFilter requestFilter)
                    {
                        requestFilter.BeforeExecute(requestContext);
                    }

                    if (!requestContext.ContinueRequest)
                    {
                        break;
                    }
                }
            }

            if (requestContext.ContinueRequest)
            {
                await invoke(requestContext);
            }

            if (requestContext.ContinueRequest)
            {
                foreach (var filter in filterList)
                {
                    if (filter is IAsyncRequestExecutionFilter asyncFilter)
                    {
                        await asyncFilter.AfterExecute(requestContext);
                    }
                    else if (filter is IRequestExecutionFilter requestFilter)
                    {
                        requestFilter.AfterExecute(requestContext);
                    }

                    if (!requestContext.ContinueRequest)
                    {
                        break;
                    }
                }
            }
        }

        public static Task SetResultTaskAsync<TResult>(Task<TResult> result, RequestExecutionContext context)
        {
            if (result.IsCompletedSuccessfully)
            {
                context.Result = result.Result;

                return Task.CompletedTask;
            }

            return CompleteTaskAsync(result, context);

            static async Task CompleteTaskAsync(Task<TResult> r, RequestExecutionContext c)
            {
                c.Result = await r;
            }
        }

        public static Task SetResultValueTaskAsync<TResult>(ValueTask<TResult> result, RequestExecutionContext context)
        {
            if (result.IsCompletedSuccessfully)
            {
                context.Result = result.Result;

                return Task.CompletedTask;
            }

            return CompleteTaskAsync(result, context);

            static async Task CompleteTaskAsync(ValueTask<TResult> r, RequestExecutionContext c)
            {
                c.Result = await r;
            }
        }
        
        public static Task WrapResult<TWrapper, TResult>(TResult result, RequestExecutionContext context) where TWrapper : IResultWrapper<TResult>, new()
        {
            context.Result = new TWrapper { Result = result };

            return Task.CompletedTask;
        }

        public static Task WrapResultTaskAsync<TWrapper, TResult>(Task<TResult> result, RequestExecutionContext context) where TWrapper : IResultWrapper<TResult> , new()
        {
            if (result.IsCompletedSuccessfully)
            {
                context.Result = new TWrapper { Result = result.Result};

                return Task.CompletedTask;
            }

            return CompleteTaskAsync(result, context);

            static async Task CompleteTaskAsync(Task<TResult> r, RequestExecutionContext c)
            {
                c.Result = new TWrapper{ Result = await r };
            }
        }

        public static Task WrapResultValueTaskAsync<TWrapper, TResult>(ValueTask<TResult> result, RequestExecutionContext context) where TWrapper : IResultWrapper<TResult>, new()
        {
            if (result.IsCompletedSuccessfully)
            {
                context.Result = new TWrapper { Result = result.Result };

                return Task.CompletedTask;
            }

            return CompleteTaskAsync(result, context);

            static async Task CompleteTaskAsync(ValueTask<TResult> r, RequestExecutionContext c)
            {
                c.Result = new TWrapper { Result = await r };
            }
        }
    }
}
