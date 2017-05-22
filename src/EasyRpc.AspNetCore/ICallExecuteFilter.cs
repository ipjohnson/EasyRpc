namespace EasyRpc.AspNetCore
{
    /// <summary>
    /// Execute filter
    /// </summary>
    public interface ICallExecuteFilter : ICallFilter
    {
        void BeforeExecute(ICallExecutionContext context);

        void AfterExecute(ICallExecutionContext context);
    }
}
