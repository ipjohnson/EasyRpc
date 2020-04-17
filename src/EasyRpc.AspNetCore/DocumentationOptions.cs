using System;
using System.Collections.Generic;
using System.Text;

namespace EasyRpc.AspNetCore
{
    /// <summary>
    /// Documentation options
    /// </summary>
    public class DocumentationOptions
    {
        /// <summary>
        /// Redirection calls to documentation
        /// </summary>
        public bool RedirectRootToDocumentation { get; set; } = true;
        
        /// <summary>
        /// url to redirect to
        /// </summary>
        public string RedirectUrl { get; set; }

        /// <summary>
        /// open api json file name
        /// </summary>
        public string OpenApiJsonUrl { get; set; } = "api.json";

        /// <summary>
        /// swagger documentation base path
        /// </summary>
        public string SwaggerBasePath { get; set; } = "/swagger/";

        /// <summary>
        /// Title for documentation
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Api description
        /// </summary>
        public string ApiDescription { get; set; }
    }
}
