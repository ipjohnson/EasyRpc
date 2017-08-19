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

        /// <summary>
        /// json rpc version
        /// </summary>
        [JsonProperty("jsonrpc", Required = Required.Always, Order = 1)]
        public string Version { get; private set; }

        /// <summary>
        /// message id
        /// </summary>
        [JsonProperty("id", Order = 3)]
        public string Id { get; private set; }
    }

    /// <summary>
    /// Empty response message
    /// </summary>
    public class EmptyResponseMessage : ResponseMessage<object>
    {
        private static readonly object EmptyObject = new object();
        
        [JsonConstructor]
        private EmptyResponseMessage() { }
            
        public EmptyResponseMessage(string version, string id) : base(EmptyObject, version, id)
        {
        }
    }

    /// <summary>
    /// Type response message
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ResponseMessage<T> : ResponseMessage
    {
        [JsonConstructor]
        protected ResponseMessage()
        {

        }
        
        /// <summary>
        /// public constructor
        /// </summary>
        /// <param name="result"></param>
        /// <param name="version"></param>
        /// <param name="id"></param>
        public ResponseMessage(T result, string version, string id) : base(version, id)
        {
            Result = result;
        }

        /// <summary>
        /// Response result
        /// </summary>
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
