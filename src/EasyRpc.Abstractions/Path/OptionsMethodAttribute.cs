using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace EasyRpc.Abstractions.Path
{
    /// <summary>
    /// OPTIONS Method
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class OptionsMethodAttribute : Attribute, IPathAttribute
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="path"></param>
        public OptionsMethodAttribute(string path = null)
        {
            Path = path;
        }

        /// <inheritdoc />
        public string Method => "OPTIONS";

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
