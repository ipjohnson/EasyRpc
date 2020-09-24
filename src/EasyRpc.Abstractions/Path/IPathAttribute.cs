using System;
using System.Collections.Generic;
using System.Text;

namespace EasyRpc.Abstractions.Path
{
    /// <summary>
    /// Attribute interface used to expose a method
    /// </summary>
    public interface IPathAttribute
    {
        /// <summary>
        /// HTTP method
        /// </summary>
        string Method { get; }

        /// <summary>
        /// Path 
        /// </summary>
        string Path { get; }

        /// <summary>
        /// HTTP success code
        /// </summary>
        int SuccessCodeValue { get; }

        /// <summary>
        /// Does the request have a body
        /// </summary>
        bool HasRequestBody { get; }

        /// <summary>
        /// Does the response have a body
        /// </summary>
        bool HasResponseBody { get; }
    }
}
