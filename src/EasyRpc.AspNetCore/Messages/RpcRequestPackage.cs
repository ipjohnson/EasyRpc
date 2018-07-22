using System;
using System.Collections.Generic;
using System.Text;
using EasyRpc.AspNetCore.Content;

namespace EasyRpc.AspNetCore.Messages
{
    public class RpcRequestPackage
    {

        public RpcRequestPackage(RpcRequestMessage message)
        {
            IsBulk = false;
            Requests = new[] { message };
        }

        public RpcRequestPackage(RpcRequestMessage[] messages)
        {
            IsBulk = true;
            Requests = messages;
        }

        public bool IsBulk { get; }

        public IEnumerable<RpcRequestMessage> Requests { get; }

        public IContentSerializer Serializer { get; set; }
    }
}
