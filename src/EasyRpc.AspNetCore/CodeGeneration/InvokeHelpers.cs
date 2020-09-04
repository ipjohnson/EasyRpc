using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using EasyRpc.AspNetCore.EndPoints;
using EasyRpc.AspNetCore.Filters;

namespace EasyRpc.AspNetCore.CodeGeneration
{
    /// <summary>
    /// static class used to invoke method delegates
    /// </summary>
    public static class InvokeHelpers
    {
        
        /// <summary>
        /// Wrap result in IResultWrapper
        /// </summary>
        /// <typeparam name="TWrapper"></typeparam>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="result"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public static Task WrapResult<TWrapper, TResult>(TResult result, RequestExecutionContext context) where TWrapper : IResultWrapper<TResult>, new()
        {
            context.Result = new TWrapper { Result = result };

            return Task.CompletedTask;
        }

        /// <summary>
        /// Wrap result from task in IResultWrapper
        /// </summary>
        /// <typeparam name="TWrapper"></typeparam>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="result"></param>
        /// <param name="context"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Wrap result using ValueTask
        /// </summary>
        /// <typeparam name="TWrapper"></typeparam>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="result"></param>
        /// <param name="context"></param>
        /// <returns></returns>
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
