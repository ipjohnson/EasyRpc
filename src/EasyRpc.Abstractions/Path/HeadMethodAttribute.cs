using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace EasyRpc.Abstractions.Path
{
    /// <summary>
    /// HEAD method
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class HeadMethodAttribute : Attribute, IPathAttribute
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="path"></param>
        public HeadMethodAttribute(string path = null)
        {
            Path = path;
        }

        /// <inheritdoc />
        public string Method => "HEAD";

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
