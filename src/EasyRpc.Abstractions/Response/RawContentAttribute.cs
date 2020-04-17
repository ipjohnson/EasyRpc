using System;
using System.Collections.Generic;
using System.Text;

namespace EasyRpc.Abstractions.Response
{
    /// <summary>
    /// Method returns raw data and should not be serialized
    /// </summary>
    public class RawContentAttribute : Attribute
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="contentType"></param>
        public RawContentAttribute(string contentType)
        {
            ContentType = contentType;
        }

        /// <summary>
        /// Content type returned
        /// </summary>
        public string ContentType { get; }

        /// <summary>
        /// Content encoding if applicable
        /// </summary>
        public string ContentEncoding { get; set; }
    }
}
