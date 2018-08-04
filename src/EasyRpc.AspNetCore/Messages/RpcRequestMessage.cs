using System;
using System.Collections.Generic;
using System.Text;
using EasyRpc.AspNetCore.Converters;
using EasyRpc.AspNetCore.Middleware;
using Newtonsoft.Json;

namespace EasyRpc.AspNetCore.Messages
{
    /// <summary>
    /// Represents an incoming rpc message
    /// </summary>
    public class RpcRequestMessage
    {
        /// <summary>
        /// json rpc version
        /// </summary>
        public string Version { get; set; }

        /// <summary>
        /// message id
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// method to call
        /// </summary>
        public string Method { get; set; }

        /// <summary>
        /// method parameters
        /// </summary>
        public object[] Parameters { get; set; }
        
        /// <summary>
        /// Error message
        /// </summary>
        public string ErrorMessage { get; set; } 

        /// <summary>
        /// Exposed method
        /// </summary>
        public IExposedMethodInformation MethodInformation { get; set; }
    }
}
