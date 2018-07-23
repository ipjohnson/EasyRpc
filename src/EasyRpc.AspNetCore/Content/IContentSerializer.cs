using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using EasyRpc.AspNetCore.Messages;
using EasyRpc.AspNetCore.Middleware;
using Microsoft.AspNetCore.Http;

namespace EasyRpc.AspNetCore.Content
{
    /// <summary>
    /// Content type serializer application/json, application/msgpack, etc.
    /// </summary>
    public interface IContentSerializer
    {
        /// <summary>
        /// ContentType to encode/decode
        /// </summary>
        string ContentType { get; }

        /// <summary>
        /// Configure content serializer
        /// </summary>
        /// <param name="configuration"></param>
        void Configure(EndPointConfiguration configuration);

        /// <summary>
        /// Seriaize the response to the outputStream
        /// </summary>
        /// <param name="outputStream"></param>
        /// <param name="response"></param>
        /// <param name="context"></param>
        void SerializeResponse(Stream outputStream, object response, HttpContext context);

        /// <summary>
        /// Deserialize the input stream to a request package
        /// </summary>
        /// <param name="inputStream">input stream</param>
        /// <param name="path"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        RpcRequestPackage DeserializeRequestPackage(Stream inputStream, string path, HttpContext context);
    }
}
