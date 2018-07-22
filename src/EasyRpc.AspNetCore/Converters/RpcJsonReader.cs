using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
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
        public RpcJsonReader(TextReader reader, string urlPath) : base(reader)
        {
            UrlPath = urlPath;
        }

        /// <summary>
        /// Url path for call
        /// </summary>
        public string UrlPath { get; }
    }
}
