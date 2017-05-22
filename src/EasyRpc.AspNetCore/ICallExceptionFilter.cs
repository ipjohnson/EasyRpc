using System;

namespace EasyRpc.AspNetCore
{
    public interface ICallExceptionFilter : ICallFilter
    {
        void HandleException(ICallExecutionContext context, Exception exception);
    }
}
