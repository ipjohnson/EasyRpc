using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EasyRpc.AspNetCore.Serializers
{
    /// <summary>
    /// Service that can be used to serialize content to the response stream 
    /// </summary>
    public interface IContentSerializationService
    {
        /// <summary>
        /// List of content types supported but serialization service
        /// </summary>
        IEnumerable<string> SupportedContentTypes { get; }

        /// <summary>
        /// Serialize result on context to response stream
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        Task SerializeToResponse(RequestExecutionContext context);

        /// <summary>
        /// Deserialize request stream to a specific type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="context"></param>
        /// <returns></returns>
        ValueTask<T> DeserializeFromRequest<T>(RequestExecutionContext context);
    }
}
