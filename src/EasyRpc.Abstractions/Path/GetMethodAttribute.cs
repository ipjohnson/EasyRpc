using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace EasyRpc.Abstractions.Path
{
    public class GetMethodAttribute : Attribute, IPathAttribute
    {
        public GetMethodAttribute(string path = null)
        {
            Path = path;
        }

        public string Method => "GET";

        public string Path { get; }

        public HttpStatusCode Success { get; set; } = HttpStatusCode.OK;

        int IPathAttribute.SuccessCodeValue => (int)Success;

        public bool HasBody => false;
    }
}
