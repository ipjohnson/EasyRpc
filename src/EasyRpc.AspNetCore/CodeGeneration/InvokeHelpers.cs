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
        /// Wraps a Task object for calling
        /// </summary>
        /// <param name="task"></param>
        /// <returns></returns>
        public static async Task<object> WrapTask(Task task)
        {
            await task;

            return null;
        }

        /// <summary>
        /// Wrap ValueTask object for calling
        /// </summary>
        /// <param name="task"></param>
        /// <returns></returns>
        public static async Task<object> WrapValueTask(ValueTask task)
        {
            await task;

            return null;
        }

        /// <summary>
        /// Wrap result in IResultWrapper
        /// </summary>
        /// <typeparam name="TWrapper"></typeparam>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="result"></param>
        /// <returns></returns>
        public static Task<TWrapper> WrapResult<TWrapper, TResult>(TResult result) where TWrapper : IResultWrapper<TResult>, new()
        {
            return Task.FromResult(new TWrapper { Result = result });
        }

        /// <summary>
        /// Wrap result from task in IResultWrapper
        /// </summary>
        /// <typeparam name="TWrapper"></typeparam>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="result"></param>
        /// <returns></returns>
        public static Task<TWrapper> WrapResultTaskAsync<TWrapper, TResult>(Task<TResult> result) where TWrapper : IResultWrapper<TResult>, new()
        {
            if (result.IsCompletedSuccessfully)
            {
                return Task.FromResult(new TWrapper { Result = result.Result });
            }

            return CompleteTaskAsync(result);

            static async Task<TWrapper> CompleteTaskAsync(Task<TResult> r)
            {
                return new TWrapper { Result = await r };
            }
        }

        /// <summary>
        /// Wrap result using ValueTask
        /// </summary>
        /// <typeparam name="TWrapper"></typeparam>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="result"></param>
        /// <returns></returns>
        public static Task<TWrapper> WrapResultValueTaskAsync<TWrapper, TResult>(ValueTask<TResult> result) where TWrapper : IResultWrapper<TResult>, new()
        {
            if (result.IsCompletedSuccessfully)
            {
                return Task.FromResult(new TWrapper { Result = result.Result });
            }

            return CompleteTaskAsync(result);

            static async Task<TWrapper> CompleteTaskAsync(ValueTask<TResult> r)
            {
                return new TWrapper { Result = await r };
            }
        }
    }
}
