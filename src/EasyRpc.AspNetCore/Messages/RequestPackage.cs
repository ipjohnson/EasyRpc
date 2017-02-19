using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EasyRpc.AspNetCore.Converters;
using Newtonsoft.Json;

namespace EasyRpc.AspNetCore.Messages
{
    [JsonConverter(typeof(RequestPackageConverter))]
    public class RequestPackage
    {
        public RequestPackage(RequestMessage message)
        {
            IsBulk = false;
            Requests = new[] {message};
        }

        public RequestPackage(RequestMessage[] messages)
        {
            IsBulk = true;
            Requests = messages;
        }

        public bool IsBulk { get; }

        public IEnumerable<RequestMessage> Requests { get; }
    }
}
