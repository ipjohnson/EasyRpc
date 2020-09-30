using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace EasyRpc.Abstractions.Path
{
    /// <summary>
    /// Expose HTTP Method
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class HttpMethodAttribute : Attribute, IPathAttribute
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="method"></param>
        /// <param name="path"></param>
        public HttpMethodAttribute(string method, string path = null)
        {
            Method = method;
            Path = path;
        }

        /// <inheritdoc />
        public string Method { get; }

        /// <inheritdoc />
        public string Path { get; }

        /// <summary>
        /// HTTP success status code
        /// </summary>
        public HttpStatusCode? Success { get; set; } = HttpStatusCode.OK;

        /// <inheritdoc />
        int? IPathAttribute.SuccessCodeValue => Success.HasValue ? (int)Success : (int?)null;

        /// <inheritdoc />
        public bool? HasRequestBody { get; set; }

        /// <inheritdoc />
        public bool? HasResponseBody { get; set; }
    }
}
