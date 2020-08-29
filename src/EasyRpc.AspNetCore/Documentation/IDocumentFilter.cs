using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.OpenApi.Models;

namespace EasyRpc.AspNetCore.Documentation
{
    /// <summary>
    /// Document filter context
    /// </summary>
    public class DocumentFilterContext
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="document"></param>
        /// <param name="httpContext"></param>
        public DocumentFilterContext(OpenApiDocument document, HttpContext httpContext)
        {
            Document = document;
            HttpContext = httpContext;
        }

        /// <summary>
        /// Http context
        /// </summary>
        public HttpContext HttpContext { get; }

        /// <summary>
        /// Open api document
        /// </summary>
        public OpenApiDocument Document { get; }
    }

    /// <summary>
    /// Document filter
    /// </summary>
    public interface IDocumentFilter
    {
        /// <summary>
        /// Apply logic to document
        /// </summary>
        /// <param name="context"></param>
        void Apply(DocumentFilterContext context);
    }
}
