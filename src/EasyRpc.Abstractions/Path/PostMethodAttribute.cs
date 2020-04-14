using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;

namespace EasyRpc.Abstractions.Path
{
    public class PostMethodAttribute : Attribute, IPathAttribute
    {
        public PostMethodAttribute(string path = null)
        {
            Path = path;
        }

        public string Method => "POST";

        public string Path { get; }

        public HttpStatusCode Success { get; set; } = HttpStatusCode.OK;

        int IPathAttribute.SuccessCodeValue => (int)Success;

        public bool HasBody => true;
    }
}
