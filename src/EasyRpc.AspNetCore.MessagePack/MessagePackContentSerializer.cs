using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using EasyRpc.AspNetCore.Content;
using EasyRpc.AspNetCore.Messages;
using EasyRpc.AspNetCore.Middleware;
using MessagePack.Formatters;
using MsgPack = MessagePack;

namespace EasyRpc.AspNetCore.MessagePack
{
    public class MessagePackContentSerializer : IContentSerializer
    {
        /// <summary>
        /// ContentType to encode/decode
        /// </summary>
        public string ContentType => "application/msgpack";

        /// <summary>
        /// Configure content serializer
        /// </summary>
        /// <param name="configuration"></param>
        public void Configure(EndPointConfiguration configuration)
        {

        }

        /// <summary>
        /// Seriaize the response to the outputStream
        /// </summary>
        /// <param name="outputStream"></param>
        /// <param name="response"></param>
        public void SerializeResponse(Stream outputStream, object response)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Deserialize the input stream to a request package
        /// </summary>
        /// <param name="inputStream">input stream</param>
        /// <param name="path"></param>
        /// <returns></returns>
        public RpcRequestPackage DeserializeRequestPackage(Stream inputStream, string path)
        {
            return MsgPack.MessagePackSerializer.Deserialize<RpcRequestPackage>(inputStream,)
        }
    }

    public class FormatterResolver : MsgPack.IFormatterResolver
    {
        public IMessagePackFormatter<T> GetFormatter<T>()
        {
            throw new NotImplementedException();
        }
    }

    public class RpcMessageFormatter : IMessagePackFormatter<RpcRequestMessage>
    {
        public int Serialize(ref byte[] bytes, int offset, RpcRequestMessage value, MsgPack.IFormatterResolver formatterResolver)
        {
            throw new NotImplementedException();
        }

        public RpcRequestMessage Deserialize(byte[] bytes, int offset, MsgPack.IFormatterResolver formatterResolver, out int readSize)
        {
            throw new NotImplementedException();
        }
    }
}
