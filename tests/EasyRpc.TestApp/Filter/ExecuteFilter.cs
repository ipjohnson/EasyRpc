using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EasyRpc.AspNetCore;
using EasyRpc.AspNetCore.Attributes;

namespace EasyRpc.TestApp.Filter
{
    public class ExecuteFilter : ICallExecuteFilter, ICallExceptionFilter
    {
        /// <summary>
        /// Called before the execution happens. 
        /// </summary>
        /// <param name="context"></param>
        public Task BeforeExecute(ICallExecutionContext context)
        {
            return Task.CompletedTask;
        }

        /// <summary>
        /// Called after the execution
        /// </summary>
        /// <param name="context"></param>
        public Task AfterExecute(ICallExecutionContext context)
        {
            return Task.CompletedTask;
        }

        /// <summary>
        /// Handle exception
        /// </summary>
        /// <param name="context"></param>
        /// <param name="exception"></param>
        public Task HandleException(ICallExecutionContext context, Exception exception)
        {
            return Task.CompletedTask;
        }
    }

    public class ExecuteFilterAttribute : RpcFilterAttribute
    {
        public ExecuteFilterAttribute() : base(typeof(ExecuteFilter))
        {

        }
    }
}
