﻿using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace EasyRpc.Abstractions.Path
{
    /// <summary>
    /// Patch HTTP method
    /// </summary>
    public class PatchMethodAttribute : Attribute, IPathAttribute
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="path"></param>
        public PatchMethodAttribute(string path = null)
        {
            Path = path;
        }


        /// <inheritdoc />
        public string Method => "PATCH";

        /// <inheritdoc />
        public string Path { get; }

        /// <summary>
        /// HTTP success status code
        /// </summary>
        public HttpStatusCode Success { get; set; } = HttpStatusCode.OK;

        /// <inheritdoc />
        int IPathAttribute.SuccessCodeValue => (int)Success;

        /// <inheritdoc />
        public bool HasBody => true;
    }
}
