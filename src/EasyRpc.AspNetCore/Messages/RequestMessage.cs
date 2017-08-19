using EasyRpc.AspNetCore.Converters;
using Newtonsoft.Json;

namespace EasyRpc.AspNetCore.Messages
{
    /// <summary>
    /// class representing the json version
    /// </summary>
    public class RequestMessage
    {
        /// <summary>
        /// json rpc version
        /// </summary>
        [JsonProperty("jsonrpc", Required = Required.Always)]
        public string Version { get; set; }

        /// <summary>
        /// message id
        /// </summary>
        [JsonProperty("id")]
        public string Id { get; set; }

        /// <summary>
        /// method to call
        /// </summary>
        [JsonProperty("method", Required = Required.Always)]
        public string Method { get; set; }

        /// <summary>
        /// method parameters
        /// </summary>
        [JsonProperty("params")]
        [JsonConverter(typeof(RpcParameterConverter))]
        public object Parameters { get; set; }
    }
}
