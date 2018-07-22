using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using EasyRpc.AspNetCore.Content;
using EasyRpc.AspNetCore.Messages;
using EasyRpc.AspNetCore.Middleware;
using FastJson = Utf8Json;

namespace EasyRpc.AspNetCore.Utf8Json
{
    public class Utf8JsonContentSerializer : IContentSerializer
    {
        /// <summary>
        /// ContentType to encode/decode
        /// </summary>
        public string ContentType => "application/json";

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

        }

        /// <summary>
        /// Deserialize the input stream to a request package
        /// </summary>
        /// <param name="inputStream">input stream</param>
        /// <param name="path"></param>
        /// <returns></returns>
        public RpcRequestPackage DeserializeRequestPackage(Stream inputStream, string path)
        {
            FastJson.JsonSerializer.Deserialize<RpcRequestPackage>(inputStream, )

            return null;
        }
    }
}
