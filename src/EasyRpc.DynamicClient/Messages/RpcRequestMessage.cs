using Newtonsoft.Json;

namespace EasyRpc.DynamicClient.Messages
{
    public class RpcRequestMessage
    {
        [JsonProperty("jsonrpc", Order = 1)]
        public string Version { get; set; } = "2.0";
        
        [JsonProperty("method", Required = Required.Always, Order = 2)]
        public string Method { get; set; }

        [JsonProperty("params", Order = 3)]
        public object Parameters { get; set; }

        [JsonProperty("id", Order = 4)]
        public string Id { get; set; }
    }
}
