using Newtonsoft.Json;

namespace EasyRpc.DynamicClient.Messages
{
    public class RpcRequestMessage
    {
        [JsonProperty("jsonrpc", Required = Required.Always)]
        public string Version { get; set; } = "2.0";

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("method", Required = Required.Always)]
        public string Method { get; set; }

        [JsonProperty("params")]
        public object Parameters { get; set; }
    }
}
