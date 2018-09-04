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
        void BeforeExecute(ICallExecutionContext context);

        /// <summary>
        /// Called after the execution
        /// </summary>
        /// <param name="context"></param>
        void AfterExecute(ICallExecutionContext context);
    }
}
