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
    public interface IDocumentationService
    {
        Task Execute(HttpContext httpContext, RequestDelegate next);

        void Configure(IInternalApiConfiguration apiInformation, IReadOnlyList<IEndPointMethodHandler> endPointMethodHandlersList);
    }

    public class DocumentationService : IDocumentationService
    {
        private readonly IOpenApiGenerationService _openApiGenerationService;
        private readonly ISwaggerAssetProvider _swaggerAssetProvider;
        private readonly IConfigurationManager _configurationManager;
        private bool _enabled;
        private string _pathBase;
        private string _jsonDataPath;
        private bool _redirect;

        public DocumentationService(IOpenApiGenerationService openApiGenerationService, ISwaggerAssetProvider swaggerAssetProvider, IConfigurationManager configurationManager)
        {
            _openApiGenerationService = openApiGenerationService;
            _swaggerAssetProvider = swaggerAssetProvider;
            _configurationManager = configurationManager;
        }

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

        private async Task HandleDocumentationRequest(HttpContext httpContext, RequestDelegate next)
        {
            if (string.Equals(httpContext.Request.Path, _jsonDataPath, StringComparison.CurrentCultureIgnoreCase))
            {
                await _openApiGenerationService.Execute(httpContext, next);
            }
            else if(!await _swaggerAssetProvider.ProvideAsset(httpContext))
            {
                await RedirectToSwagger(httpContext);
            }
        }

        private async Task RedirectToSwagger(HttpContext httpContext)
        {
            httpContext.Response.Headers["Location"] = _pathBase + "index.html";
            httpContext.Response.StatusCode = (int) HttpStatusCode.Redirect;
        }

        public void Configure(IInternalApiConfiguration apiInformation, IReadOnlyList<IEndPointMethodHandler> endPointMethodHandlersList)
        {
            var documentationOptions = _configurationManager.GetConfiguration<DocumentationOptions>();

            _enabled = documentationOptions.Enabled;

            if (_enabled)
            {
                _redirect = documentationOptions.RedirectRootToDocumentation;
                _jsonDataPath = documentationOptions.SwaggerBasePath + documentationOptions.OpenApiJsonUrl;
                _pathBase = documentationOptions.SwaggerBasePath;

                _openApiGenerationService.Configure(apiInformation, documentationOptions, endPointMethodHandlersList);

                _swaggerAssetProvider.Configure();
            }
        }
    }
}
