using System.IO;
using System.IO.Compression;
using EasyRpc.AspNetCore.Content;

namespace EasyRpc.AspNetCore.Brotli
{
    public class BrotliContentEncoder : IContentEncoder
    {
        /// <summary>
        /// Content Encoding
        /// </summary>
        public string ContentEncoding => "br";

        /// <summary>
        /// Provide a wrapper stream that endcodes the contentStream
        /// </summary>
        /// <param name="contentStream">stream to encode</param>
        /// <returns></returns>
        public Stream EncodeStream(Stream contentStream)
        {
            return new BrotliStream(contentStream, CompressionLevel.Fastest);
        }

        /// <summary>
        /// Provide a wrapper stream that decodes the contentStream
        /// </summary>
        /// <param name="contentStream">stream to decode</param>
        /// <returns></returns>
        public Stream DecodeStream(Stream contentStream)
        {
            return new BrotliStream(contentStream, CompressionMode.Decompress);
        }
    }
}
