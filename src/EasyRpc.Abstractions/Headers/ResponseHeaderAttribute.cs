using System;
using System.Collections.Generic;
using System.Text;

namespace EasyRpc.Abstractions.Headers
{
    /// <summary>
    /// Sets a response header value
    /// </summary>
    public class ResponseHeaderAttribute : Attribute
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        public ResponseHeaderAttribute(string name, string value)
        {
            Name = name;
            Value = value;
        }

        /// <summary>
        /// Header name (i.e. Cache-Control, etc)
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Value for header
        /// </summary>
        public string Value { get; }
    }
}
