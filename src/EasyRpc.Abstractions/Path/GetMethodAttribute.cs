using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace EasyRpc.Abstractions.Path
{
    /// <summary>
    /// Expose method as GET 
    /// </summary>
    public class GetMethodAttribute : Attribute, IPathAttribute
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="path"></param>
        public GetMethodAttribute(string path = null)
        {
            Path = path;
        }

        /// <inheritdoc />
        public string Method => "GET";

        /// <inheritdoc />
        public string Path { get; }

        /// <summary>
        /// HTTP success status code
        /// </summary>
        public HttpStatusCode Success { get; set; } = HttpStatusCode.OK;

        /// <inheritdoc />
        int IPathAttribute.SuccessCodeValue => (int)Success;

        /// <inheritdoc />
        public bool HasBody => false;
    }
}
