using System;
using EasyRpc.AspNetCore.Messages;
using Microsoft.AspNetCore.Http;

namespace EasyRpc.AspNetCore
{
    public interface ICallExecutionContext
    {
        /// <summary>
        /// Should the call continue
        /// </summary>
        bool ContinueCall { get; set; }

        /// <summary>
        /// HttpContext for call
        /// </summary>
        HttpContext Context { get; }

        /// <summary>
        /// Class being executed
        /// </summary>
        Type ExecutingClass { get; }

        /// <summary>
        /// Call parameters
        /// </summary>
        object[] Parameters { get; }

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
