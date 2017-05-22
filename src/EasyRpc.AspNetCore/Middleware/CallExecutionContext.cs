using System;
using EasyRpc.AspNetCore.Messages;
using Microsoft.AspNetCore.Http;

namespace EasyRpc.AspNetCore.Middleware
{
    public class CallExecutionContext : ICallExecutionContext
    {
        public CallExecutionContext(HttpContext context, Type executingClass, RequestMessage requestMessage)
        {
            ContinueCall = true;
            Context = context;
            ExecutingClass = executingClass;
            RequestMessage = requestMessage;
        }

        public HttpContext Context { get; }

        public Type ExecutingClass { get; }

        public RequestMessage RequestMessage { get; }

        public bool ContinueCall { get; set; }

        public ResponseMessage ResponseMessage { get; set; }
    }
}
