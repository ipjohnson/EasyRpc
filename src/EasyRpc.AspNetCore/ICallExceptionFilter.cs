using System;

namespace EasyRpc.AspNetCore
{
    /// <summary>
    /// Filter that is invoked when an exception happens
    /// </summary>
    public interface ICallExceptionFilter : ICallFilter
    {
        /// <summary>
        /// Handle exception
        /// </summary>
        /// <param name="context"></param>
        /// <param name="exception"></param>
        void HandleException(ICallExecutionContext context, Exception exception);
    }
}
