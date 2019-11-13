using System.IO;
using System.Threading.Tasks;
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
        /// Id assigned by framework to serializer
        /// </summary>
        int SerializerId { get; set; }

        /// <summary>
        /// Content type for serializer
        /// </summary>
        string ContentType { get; }

        /// <summary>
        /// Can serialize
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        bool CanSerialize(HttpContext context);

        /// <summary>
        /// Configure content serializer
        /// </summary>
        void Configure(IExposeMethodInformationCacheManager cacheManager);

        /// <summary>
        /// Serialize the response to the outputStream
        /// </summary>
        /// <param name="outputStream"></param>
        /// <param name="response"></param>
        /// <param name="context"></param>
        Task SerializeResponse(Stream outputStream, object response, HttpContext context);

        /// <summary>
        /// Deserialize the input stream to a request package
        /// </summary>
        /// <param name="inputStream">input stream</param>
        /// <param name="path"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        Task<RpcRequestPackage> DeserializeRequestPackage(Stream inputStream, string path, HttpContext context);
    }
}
