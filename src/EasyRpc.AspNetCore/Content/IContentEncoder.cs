using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using EasyRpc.AspNetCore.Middleware;

namespace EasyRpc.AspNetCore.Content
{
    /// <summary>
    /// Content encoder gzip, br, deflate, etc
    /// </summary>
    public interface IContentEncoder
    {
        /// <summary>
        /// Content Encoding
        /// </summary>
        string ContentEncoding { get; }

        /// <summary>
        /// Provide a wrapper stream that endcodes the contentStream
        /// </summary>
        /// <param name="contentStream">stream to encode</param>
        /// <returns></returns>
        Stream EncodeStream(Stream contentStream);

        /// <summary>
        /// Provide a wrapper stream that decodes the contentStream
        /// </summary>
        /// <param name="contentStream">stream to decode</param>
        /// <returns></returns>
        Stream DecodeStream(Stream contentStream);
    }
}
