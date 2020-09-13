using System;
using System.Collections.Generic;
using System.Text;
using EasyRpc.AspNetCore.Documentation;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;

namespace EasyRpc.AspNetCore
{
    /// <summary>
    /// Documentation options
    /// </summary>
    public class DocumentationOptions
    {
        private readonly Dictionary<Type,Func<OpenApiSchema>> _knownTypes = new Dictionary<Type, Func<OpenApiSchema>>();

        /// <summary>
        /// Documentation enabled, true by default
        /// </summary>
        public bool Enabled { get; set; } = true;

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
        public string UIBasePath { get; set; } = "/swagger/";

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
        /// Add extra css can be url or &lt;link&gt; tag
        /// </summary>
        public string ExtraCss { get; set; }

        /// <summary>
        /// Add extra JavaScript can be url or &lt;script&gt; tag
        /// </summary>
        public string ExtraJavaScript { get; set; }

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

        /// <summary>
        /// Path for providing content in addition or overriding the standard documentation assets
        /// </summary>
        public string ContentPath { get; set; } = "wwwroot/swagger-ui/";

        /// <summary>
        /// Define how a type is represented in swagger document
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="schemaFunc"></param>
        /// <returns></returns>
        public DocumentationOptions MapType<T>(Func<OpenApiSchema> schemaFunc)
        {
            return MapType(typeof(T), schemaFunc);
        }

        /// <summary>
        /// Map a string type with a format
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="format"></param>
        /// <param name="example"></param>
        /// <returns></returns>
        public DocumentationOptions MapStringType<T>(string format = null, string example = "")
        {
            return MapType(typeof(T), () => new OpenApiSchema {Type = "string", Format = format, Example = new OpenApiString(example)});
        }

        /// <summary>
        /// Define how a type is represented in swagger document
        /// </summary>
        /// <param name="type"></param>
        /// <param name="schemaFunc"></param>
        /// <returns></returns>
        public DocumentationOptions MapType(Type type, Func<OpenApiSchema> schemaFunc)
        {
            _knownTypes[type] = schemaFunc;

            return this;
        }

        /// <summary>
        /// Dictionary of know type mappings
        /// </summary>
        public IReadOnlyDictionary<Type, Func<OpenApiSchema>> TypeMappings => _knownTypes;

        /// <summary>
        /// Document filters
        /// </summary>
        public IList<IDocumentFilter> DocumentFilters { get; } = new List<IDocumentFilter>();

        /// <summary>
        /// Operation filters
        /// </summary>
        public IList<IOperationFilter> OperationFilters { get; } = new List<IOperationFilter>();

        /// <summary>
        /// Auto tag methods if there is no tag.
        /// </summary>
        public bool AutoTag { get; set; } = true;
    }
}
