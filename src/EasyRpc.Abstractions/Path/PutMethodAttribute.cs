using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace EasyRpc.Abstractions.Path
{
    /// <summary>
    /// PUT method
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class PutMethodAttribute : Attribute, IPathAttribute
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="path"></param>
        public PutMethodAttribute(string path = null)
        {
            Path = path;
        }
        
        /// <inheritdoc />
        public string Method => "PUT";

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
