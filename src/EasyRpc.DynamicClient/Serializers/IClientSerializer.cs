using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace EasyRpc.DynamicClient.Serializers
{
    /// <summary>
    /// Serializer 
    /// </summary>
    public interface IClientSerializer
    {
        /// <summary>
        /// Content type of serializer
        /// </summary>
        string ContentType { get; }

        /// <summary>
        /// Serialize body to request
        /// </summary>
        /// <param name="body"></param>
        /// <param name="request"></param>
        /// <param name="compressBody"></param>
        Task SerializeToRequest(object body, HttpRequestMessage request, bool compressBody);

        /// <summary>
        /// Deserialize response body
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="responseMessage"></param>
        /// <returns></returns>
        Task<T> DeserializeFromResponse<T>(HttpResponseMessage responseMessage);

        /// <summary>
        /// Serialize object
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        Task<byte[]> Serialize(object value);

        /// <summary>
        /// Deserialize from bytes
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="bytes"></param>
        /// <returns></returns>
        Task<T> Deserialize<T>(byte[] bytes);
    }
}
