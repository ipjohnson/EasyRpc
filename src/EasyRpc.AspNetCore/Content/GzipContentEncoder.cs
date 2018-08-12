using System.IO;
using System.IO.Compression;

namespace EasyRpc.AspNetCore.Content
{
    public class GzipContentEncoder : IContentEncoder
    {
        /// <summary>
        /// Content Encoding
        /// </summary>
        public string ContentEncoding => "gzip";

        /// <summary>
        /// Provide a wrapper stream that endcodes the contentStream
        /// </summary>
        /// <param name="contentStream">stream to encode</param>
        /// <returns></returns>
        public Stream EncodeStream(Stream contentStream)
        {
            return new GZipStream(contentStream, CompressionLevel.Fastest);
        }

        /// <summary>
        /// Provide a wrapper stream that decodes the contentStream
        /// </summary>
        /// <param name="contentStream">stream to decode</param>
        /// <returns></returns>
        public Stream DecodeStream(Stream contentStream)
        {
            return new GZipStream(contentStream, CompressionMode.Decompress);
        }
    }
}
