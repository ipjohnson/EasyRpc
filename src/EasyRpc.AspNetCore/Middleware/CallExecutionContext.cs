using System;
using System.Reflection;
using EasyRpc.AspNetCore.Messages;
using Microsoft.AspNetCore.Http;

namespace EasyRpc.AspNetCore.Middleware
{
    public class CallExecutionContext : ICallExecutionContext
    {
        public CallExecutionContext(HttpContext context, Type executingClass, MethodInfo method, RequestMessage requestMessage, object instance)
        {
            ContinueCall = true;
            Context = context;
            ExecutingClass = executingClass;
            ExecutingMethod = method;
            RequestMessage = requestMessage;
            Instance = instance;
        }

        public HttpContext Context { get; }

        public Type ExecutingClass { get; }
        
        public MethodInfo ExecutingMethod { get; }

        public object[] Parameters { get; set; }

        public RequestMessage RequestMessage { get; }
        
        public object Instance { get; }

        public bool ContinueCall { get; set; }

        public ResponseMessage ResponseMessage { get; set; }
    }
}
