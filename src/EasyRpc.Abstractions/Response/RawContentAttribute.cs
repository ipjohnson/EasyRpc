using System;
using System.Collections.Generic;
using System.Text;

namespace EasyRpc.Abstractions.Response
{
    /// <summary>
    /// Raw content attribute interface
    /// </summary>
    public interface IRawContentAttribute
    {

        /// <summary>
        /// Content type returned
        /// </summary>
        string ContentType { get; }

        /// <summary>
        /// Content encoding if applicable
        /// </summary>
        string ContentEncoding { get; }

    }

    /// <summary>
    /// Method returns raw data and should not be serialized
    /// </summary>
    public class RawContentAttribute : Attribute, IRawContentAttribute
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="contentType"></param>
        public RawContentAttribute(string contentType)
        {
            ContentType = contentType;
        }

        /// <inheritdoc />
        public string ContentType { get; }

        /// <inheritdoc />
        public string ContentEncoding { get; set; }
    }
}
