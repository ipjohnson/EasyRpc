using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.OpenApi.Models;

namespace EasyRpc.AspNetCore.Documentation
{
    /// <summary>
    /// Operation filter context
    /// </summary>
    public class OperationFilterContext
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="httpContext"></param>
        /// <param name="methodConfiguration"></param>
        /// <param name="operation"></param>
        /// <param name="document"></param>
        public OperationFilterContext(HttpContext httpContext, IEndPointMethodConfigurationReadOnly methodConfiguration,
            OpenApiOperation operation, OpenApiDocument document)
        {
            HttpContext = httpContext;
            MethodConfiguration = methodConfiguration;
            Operation = operation;
            Document = document;
        }

        /// <summary>
        /// HttpContext for call
        /// </summary>
        public HttpContext HttpContext { get; }

        /// <summary>
        /// Method configuration
        /// </summary>
        public IEndPointMethodConfigurationReadOnly MethodConfiguration { get; }

        /// <summary>
        /// Document this will go in
        /// </summary>
        public OpenApiDocument Document { get; }

        /// <summary>
        /// Api operation
        /// </summary>
        public OpenApiOperation Operation { get; }

    }

    /// <summary>
    /// Operation filter
    /// </summary>
    public interface IOperationFilter
    {
        /// <summary>
        /// Apply logic
        /// </summary>
        /// <param name="context"></param>
        void Apply(OperationFilterContext context);
    }
}
