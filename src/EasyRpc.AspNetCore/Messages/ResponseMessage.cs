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

    public class ResponseMessage<T> : ResponseMessage
    {
        [JsonConstructor]
        private ResponseMessage()
        {

        }

        public ResponseMessage(T result, string version, string id) : base(version, id)
        {
            Result = result;
        }

        [JsonProperty("result", Required = Required.Always, Order = 2)]
        public T Result { get; private set; }
    }

    public class ErrorResponseMessage : ResponseMessage
    {
        public class ErrorClass
        {
            [JsonProperty("code")]
            public int Code { get; set; }

            [JsonProperty("message")]
            public string Message { get; set; }
        }

        [JsonProperty("error", Order = 2)]
        public ErrorClass Error { get; set; }
    }
}
