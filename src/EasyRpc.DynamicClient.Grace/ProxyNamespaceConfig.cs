using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Reflection;
using System.Text;
using EasyRpc.DynamicClient.Serializers;

namespace EasyRpc.DynamicClient.Grace
{
    public class ProxyNamespaceConfig
    {
        public string Url { get; set; }

        public bool UseDataContext { get; set; }

        public string[] Namespaces { get; set; }

        public IClientSerializer Serializer { get; set; }

        public Func<string, HttpClient> CreateClient { get; set; }

        public Func<MethodInfo, bool> CompressRequest { get; set; }

        public Func<MethodInfo, bool> CompressResponse { get; set; }
    }
}
