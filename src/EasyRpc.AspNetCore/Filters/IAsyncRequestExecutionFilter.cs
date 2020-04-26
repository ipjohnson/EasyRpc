using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EasyRpc.AspNetCore.Filters
{
    /// <summary>
    /// Asynchronous execution filter
    /// </summary>
    public interface IAsyncRequestExecutionFilter : IRequestFilter
    {
        /// <summary>
        /// Called before the execution happens. 
        /// </summary>
        /// <param name="context"></param>
        Task BeforeExecute(RequestExecutionContext context);

        /// <summary>
        /// Called after the execution
        /// </summary>
        /// <param name="context"></param>
        Task AfterExecute(RequestExecutionContext context);
    }
}
