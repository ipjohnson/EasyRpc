using System;
using System.Threading.Tasks;

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
        Task HandleException(ICallExecutionContext context, Exception exception);
    }
}
