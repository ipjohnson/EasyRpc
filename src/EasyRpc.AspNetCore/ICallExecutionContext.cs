using System;
using EasyRpc.AspNetCore.Messages;
using Microsoft.AspNetCore.Http;

namespace EasyRpc.AspNetCore
{
    public interface ICallExecutionContext
    {
        bool ContinueCall { get; set; }

        HttpContext Context { get; }

        Type ExecutingClass { get; }

        RequestMessage RequestMessage { get; }

        ResponseMessage ResponseMessage { get; set; }
    }
}
