using System;
using System.Collections.Generic;
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

        /// <inheritdoc />
        public int SuccessCodeValue { get; set; }

        /// <inheritdoc />
        public bool HasBody { get; set; }
    }
}
