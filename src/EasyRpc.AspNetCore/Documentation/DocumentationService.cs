using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using EasyRpc.AspNetCore.Configuration;
using EasyRpc.AspNetCore.EndPoints;
using Microsoft.AspNetCore.Http;

namespace EasyRpc.AspNetCore.Documentation
{
    /// <summary>
    /// Service for serving up documentation assets
    /// </summary>
    public interface IDocumentationService
    {
        /// <summary>
        /// Execute documentation handler
        /// </summary>
        /// <param name="httpContext"></param>
        /// <param name="next"></param>
        /// <returns></returns>
        Task Execute(HttpContext httpContext, RequestDelegate next);

        /// <summary>
        /// Configure documentation service
        /// </summary>
        /// <param name="apiInformation"></param>
        /// <param name="endPointMethodHandlersList"></param>
        void Configure(IInternalApiConfiguration apiInformation, IReadOnlyList<IEndPointMethodHandler> endPointMethodHandlersList);
    }

    /// <inheritdoc />
    public class DocumentationService : IDocumentationService
    {
        private readonly IOpenApiGenerationService _openApiGenerationService;
        private readonly ISwaggerStaticResourceProvider _swaggerAssetProvider;
        private readonly IConfigurationManager _configurationManager;
        private bool _enabled;
        private string _pathBase;
        private string _jsonDataPath;
        private bool _redirect;

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="openApiGenerationService"></param>
        /// <param name="swaggerAssetProvider"></param>
        /// <param name="configurationManager"></param>
        public DocumentationService(IOpenApiGenerationService openApiGenerationService, ISwaggerStaticResourceProvider swaggerAssetProvider, IConfigurationManager configurationManager)
        {
            _openApiGenerationService = openApiGenerationService;
            _swaggerAssetProvider = swaggerAssetProvider;
            _configurationManager = configurationManager;
        }

        /// <inheritdoc />
        public Task Execute(HttpContext httpContext, RequestDelegate next)
        {
            if (!_enabled)
            {
                return next(httpContext);
            }

            if (httpContext.Request.Method == HttpMethods.Get &&
                string.Compare(httpContext.Request.Path, 0, _pathBase, 0, _pathBase.Length,
                StringComparison.CurrentCultureIgnoreCase) == 0)
            {
                return HandleDocumentationRequest(httpContext, next);
            }
            
            if (_redirect && 
                httpContext.Request.Method == HttpMethods.Get)
            {
                httpContext.Response.Headers["Location"] = _pathBase + "index.html";
                httpContext.Response.StatusCode = (int)HttpStatusCode.Redirect;

                return Task.CompletedTask;
            }

            return next(httpContext);
        }

        /// <inheritdoc />
        public void Configure(IInternalApiConfiguration apiInformation, IReadOnlyList<IEndPointMethodHandler> endPointMethodHandlersList)
        {
            var documentationOptions = _configurationManager.GetConfiguration<DocumentationOptions>();

            _enabled = documentationOptions.Enabled;

            if (_enabled)
            {
                _redirect = documentationOptions.RedirectRootToDocumentation;
                _jsonDataPath = documentationOptions.UIBasePath + documentationOptions.OpenApiJsonUrl;
                _pathBase = documentationOptions.UIBasePath;

                _openApiGenerationService.Configure(documentationOptions, endPointMethodHandlersList);

                //_swaggerAssetProvider.Configure(documentationOptions);
            }
        }

        private async Task HandleDocumentationRequest(HttpContext httpContext, RequestDelegate next)
        {
            if (string.Equals(httpContext.Request.Path.Value, _jsonDataPath, StringComparison.CurrentCultureIgnoreCase))
            {
                await _openApiGenerationService.Execute(httpContext);
            }
            //else if (!await _swaggerAssetProvider.ProvideAsset(httpContext))
            //{
            //    await RedirectToSwagger(httpContext);
            //}
        }

        private Task RedirectToSwagger(HttpContext httpContext)
        {
            httpContext.Response.Headers["Location"] = _pathBase + "index.html";
            httpContext.Response.StatusCode = (int)HttpStatusCode.Redirect;

            return Task.CompletedTask;
        }

    }
}
