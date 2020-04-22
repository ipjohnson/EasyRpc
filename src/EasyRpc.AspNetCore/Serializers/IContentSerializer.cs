using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EasyRpc.AspNetCore.Serializers
{
    /// <summary>
    /// Interface for content serializer
    /// </summary>
    public interface IContentSerializer
    {
        /// <summary>
        /// List of content types supported (mime types: application/json, etc)
        /// </summary>
        IEnumerable<string> SupportedContentTypes { get; }

        /// <summary>
        /// Should this serializer be user when no others match exactly.
        /// </summary>
        bool IsDefault { get; }

        /// <summary>
        /// Unique Id that can be used to store serializer specific info on an endpoint
        /// </summary>
        /// <param name="id"></param>
        void AssignUniqueId(int id);

        /// <summary>
        /// Serializer should be used for this call
        /// </summary>
        /// <param name="context"></param>
        /// <param name="contentType"></param>
        /// <returns></returns>
        bool CanDeserialize(RequestExecutionContext context, string contentType);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <param name="accepts"></param>
        /// <returns></returns>
        bool CanSerialize(RequestExecutionContext context, string accepts);

        /// <summary>
        /// Serialize the result on the rpc context to the http response
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        Task SerializeToResponse(RequestExecutionContext context);

        /// <summary>
        /// Deserialize http request body to rpc context
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        ValueTask<T> DeserializeFromRequest<T>(RequestExecutionContext context);
    }
}
