using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace EasyRpc.AspNetCore.Messages
{
    public class ResponseMessage
    {
        [JsonConstructor]
        protected ResponseMessage()
        {

        }

        public ResponseMessage(string version, string id)
        {
            Version = version;
            Id = id;
        }

        [JsonProperty("jsonrpc", Required = Required.Always, Order = 1)]
        public string Version { get; private set; }

        [JsonProperty("id", Order = 3)]
        public string Id { get; private set; }
    }

    public class EmptyResponseMessage : ResponseMessage<object>
    {
        private static readonly object _emptyObject = new object();
        
        [JsonConstructor]
        private EmptyResponseMessage() { }
            
        public EmptyResponseMessage(string version, string id) : base(_emptyObject, version, id)
        {
        }
    }

    public class ResponseMessage<T> : ResponseMessage
    {
        [JsonConstructor]
        protected ResponseMessage()
        {

        }

        public ResponseMessage(T result, string version, string id) : base(version, id)
        {
            Result = result;
        }

        [JsonProperty("result", Required = Required.Always, Order = 2)]
        public T Result { get; private set; }
    }

    /// <summary>
    /// error codes from http://www.jsonrpc.org/specification#error_object
    /// </summary>
    public enum JsonRpcErrorCode
    {
        /// <summary>
        /// Parse error
        /// </summary>
        ParseError = -32700,

        /// <summary>
        /// Invalid request
        /// </summary>
        InvalidRequest = -32600,

        /// <summary>
        /// Method Not Found
        /// </summary>
        MethodNotFound = -32601,

        /// <summary>
        /// Internal server error
        /// </summary>
        InternalServerError = -32603,

        /// <summary>
        /// Unauthorized access
        /// </summary>
        UnauthorizedAccess = -32000
    }

    public class ErrorResponseMessage : ResponseMessage
    {
        [JsonConstructor]
        private ErrorResponseMessage() { }

        public ErrorResponseMessage(string version, string id, JsonRpcErrorCode errorCode, string errorMessage) : base(version, id)
        {
            Error = new ErrorClass { Code = (int)errorCode, Message = errorMessage };
        }

        public class ErrorClass
        {
            [JsonProperty("code")]
            public int Code { get; set; }

            [JsonProperty("message")]
            public string Message { get; set; }
        }

        [JsonProperty("error", Order = 2)]
        public ErrorClass Error { get; private set; }
    }
}
