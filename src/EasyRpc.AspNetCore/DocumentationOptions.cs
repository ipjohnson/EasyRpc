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
        /// Documentation version, defaults to assembly version if not provided
        /// </summary>
        public string Version { get; set; }

        /// <summary>
        /// Func used to set version string in Documentation
        /// </summary>
        public Func<Version, string> VersionFormat { get; set; } = DefaultVersionFormat;

        /// <summary>
        /// Default version format 1.0.0
        /// </summary>
        /// <param name="version"></param>
        /// <returns></returns>
        public static string DefaultVersionFormat(Version version)
        {
            return $"{version.Major}.{version.Minor}.{version.Build}";
        }

        /// <summary>
        /// Api description
        /// </summary>
        public string ApiDescription { get; set; }
    }
}
