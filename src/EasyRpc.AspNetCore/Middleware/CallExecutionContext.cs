using System;
using System.Reflection;
using EasyRpc.AspNetCore.Messages;
using Microsoft.AspNetCore.Http;

namespace EasyRpc.AspNetCore.Middleware
{
    public class CallExecutionContext : ICallExecutionContext
    {
        public CallExecutionContext(HttpContext context, Type executingClass, MethodInfo method, RpcRequestMessage requestMessage)
        {
            ContinueCall = true;
            HttpContext = context;
            ExecutingClass = executingClass;
            ExecutingMethod = method;
            RequestMessage = requestMessage;
        }

        public HttpContext HttpContext { get; }

        public Type ExecutingClass { get; }
        
        public MethodInfo ExecutingMethod { get; }

        public object[] Parameters { get; set; }

        public RpcRequestMessage RequestMessage { get; }
        
        public object Instance { get; set; }

        public bool ContinueCall { get; set; }

        public ResponseMessage ResponseMessage { get; set; }
    }
}
