using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EasyRpc.AspNetCore.Converters;
using Newtonsoft.Json;

namespace EasyRpc.AspNetCore.Messages
{
    public class RequestMessage
    {
        [JsonProperty("jsonrpc", Required = Required.Always)]
        public string Version { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("method", Required = Required.Always)]
        public string Method { get; set; }

        [JsonProperty("params")]
        [JsonConverter(typeof(RpcParameterConverter))]
        public object Parameters { get; set; }
    }
}
