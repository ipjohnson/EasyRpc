using System;
using System.Collections.Generic;
using System.Text;

namespace EasyRpc.Abstractions.Response
{
    public class RawContentAttribute : Attribute
    {
        public RawContentAttribute(string contentType)
        {
            ContentType = contentType;
        }

        public string ContentType { get; }

        public string ContentEncoding { get; set; }
    }
}
