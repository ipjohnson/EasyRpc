using System;
using System.Reflection;
using EasyRpc.AspNetCore.Messages;
using Microsoft.AspNetCore.Http;

namespace EasyRpc.AspNetCore
{
    public interface ICallExecutionContext
    {
        /// <summary>
        /// Instance that the execution will happen
        /// </summary>
        object Instance { get; }

        /// <summary>
        /// Should the call continue
        /// </summary>
        bool ContinueCall { get; set; }

        /// <summary>
        /// HttpContext for call
        /// </summary>
        HttpContext HttpContext { get; }

        /// <summary>
        /// Class being executed
        /// </summary>
        Type ExecutingClass { get; }

        /// <summary>
        /// Method being executed
        /// </summary>
        MethodInfo ExecutingMethod { get; }

        /// <summary>
        /// Call parameters
        /// </summary>
        object[] Parameters { get; set; }

        /// <summary>
        /// Request message
        /// </summary>
        RequestMessage RequestMessage { get; }

        /// <summary>
        /// Response message
        /// </summary>
        ResponseMessage ResponseMessage { get; set; }
    }
}
