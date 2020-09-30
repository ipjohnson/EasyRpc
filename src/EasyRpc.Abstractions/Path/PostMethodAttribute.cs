using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;

namespace EasyRpc.Abstractions.Path
{
    /// <summary>
    /// Expose method as POST
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class PostMethodAttribute : Attribute, IPathAttribute
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="path"></param>
        public PostMethodAttribute(string path = null)
        {
            Path = path;
        }


        /// <inheritdoc />
        public string Method => "POST";

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
