using Newtonsoft.Json;

namespace EasyRpc.AspNetCore.Messages
{
    public class ResponseMessage
    {
        /// <summary>
        /// Return this response if you want the framework to not write anything to the resposne stream
        /// this is intended if you want to write something other than application/json to the Response stream
        /// </summary>
        public static ResponseMessage NoResponse = new ResponseMessage();
        
        [JsonConstructor]
        protected ResponseMessage()
        {

        }

        public ResponseMessage(string version = "2.0", string id = "")
        {
            Version = version;
            Id = id;
        }

        /// <summary>
        /// json rpc version
        /// </summary>
        [JsonProperty("jsonrpc", Required = Required.Always, Order = 1)]
        public string Version { get; set; }

        /// <summary>
        /// message id
        /// </summary>
        [JsonProperty("id", Order = 3)]
        public string Id { get; set; }

        /// <summary>
        /// Should this response be compressed
        /// </summary>
        [JsonIgnore]
        public bool CanCompress { get; set; }
    }

    /// <summary>
    /// Empty response message
    /// </summary>
    public class EmptyResponseMessage : ResponseMessage<object>
    {
        private static readonly object EmptyObject = new object();
        
        [JsonConstructor]
        private EmptyResponseMessage() { }
            
        public EmptyResponseMessage(string version = "2.0", string id = "") : base(EmptyObject, version, id)
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
        public ResponseMessage(T result, string version = "2.0", string id = "") : base(version, id)
        {
            Result = result;
        }

        /// <summary>
        /// Response result
        /// </summary>
        [JsonProperty("result", Order = 2)]
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

        public ErrorResponseMessage(JsonRpcErrorCode errorCode, string errorMessage, string version = "2.0", string id = "") : base(version, id)
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
