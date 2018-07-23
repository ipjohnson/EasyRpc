using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace EasyRpc.AspNetCore.Converters
{
    public class RpcJsonReader : JsonTextReader
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="T:Newtonsoft.Json.JsonTextReader" /> class with the specified <see cref="T:System.IO.TextReader" />.
        /// </summary>
        /// <param name="reader">The <see cref="T:System.IO.TextReader" /> containing the JSON data to read.</param>
        /// <param name="urlPath"></param>
        /// <param name="context"></param>
        public RpcJsonReader(TextReader reader, string urlPath, HttpContext context) : base(reader)
        {
            UrlPath = urlPath;
            Context = context;
        }

        /// <summary>
        /// Url path for call
        /// </summary>
        public string UrlPath { get; }

        /// <summary>
        /// HttpContext for the call
        /// </summary>
        public HttpContext Context { get; }
    }
}
