using System.Threading.Tasks;

namespace EasyRpc.AspNetCore
{
    /// <summary>
    /// Execute filter
    /// </summary>
    public interface ICallExecuteFilter : ICallFilter
    {
        /// <summary>
        /// Called before the execution happens. 
        /// </summary>
        /// <param name="context"></param>
        Task BeforeExecute(ICallExecutionContext context);

        /// <summary>
        /// Called after the execution
        /// </summary>
        /// <param name="context"></param>
        Task AfterExecute(ICallExecutionContext context);
    }
}
