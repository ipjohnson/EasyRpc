using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EasyRpc.AspNetCore.Configuration;
using EasyRpc.AspNetCore.EndPoints;
using EasyRpc.AspNetCore.Routing;
using Microsoft.AspNetCore.Http;

namespace EasyRpc.AspNetCore.Documentation
{
    public class DocumentationEndPointInspector : IApiEndPointInspector
    {
        private readonly IOpenApiGenerationService _openApiGenerationService;
        private readonly ISwaggerStaticResourceProvider _swaggerStaticResourceProvider;
        private readonly EndPointServices _endPointServices;
        private readonly IConfigurationManager _configurationManager;
        private string _baseDocumentationPath;
        private string _basePath;

        public DocumentationEndPointInspector(IOpenApiGenerationService openApiGenerationService, ISwaggerStaticResourceProvider swaggerStaticResourceProvider, EndPointServices endPointServices, IConfigurationManager configurationManager)
        {
            _openApiGenerationService = openApiGenerationService;
            _swaggerStaticResourceProvider = swaggerStaticResourceProvider;
            _endPointServices = endPointServices;
            _configurationManager = configurationManager;
        }

        public IEnumerable<IEndPointMethodHandler> InspectEndPoints(Dictionary<string, Dictionary<string, IEndPointMethodHandler>> endPoints)
        {
            var returnList = new List<IEndPointMethodHandler>();

            var documentationOptions = _configurationManager.GetConfiguration<DocumentationOptions>();

            if (!documentationOptions.Enabled)
            {
                return returnList;
            }

            SetBasePath(documentationOptions);

            var endPointList = endPoints.Values.SelectMany(dict => dict.Values).ToList();

            _openApiGenerationService.Configure(documentationOptions, endPointList);


            returnList.Add(CreateOpenApiJsonEndPoint(documentationOptions));

            returnList.AddRange(CreateSwaggerStaticResourceEndPoints(documentationOptions));

            if (documentationOptions.RedirectRootToDocumentation && 
                !endPoints.ContainsKey(_basePath))
            {
                returnList.Add(RedirectEndPoint(documentationOptions));
            }

            return returnList;
        }


        private void SetBasePath(DocumentationOptions documentationOptions)
        {
            _basePath = _configurationManager.GetConfiguration<BasePathOptions>().Path;

            if (!_basePath.EndsWith('/'))
            {
                _basePath += '/';
            }

            _baseDocumentationPath = _basePath + documentationOptions.UIBasePath.TrimStart('/');
        }

        private IEnumerable<IEndPointMethodHandler> CreateSwaggerStaticResourceEndPoints(DocumentationOptions documentationOptions)
        {
            foreach (var staticResource in _swaggerStaticResourceProvider.ProvideStaticResources(documentationOptions))
            {
                var configuration = CreateEndPointMethodConfiguration(_baseDocumentationPath + staticResource.Path);

                yield return new StaticResourceEndPointMethodHandler(_endPointServices, configuration, staticResource);
            }
        }

        private IEndPointMethodHandler CreateOpenApiJsonEndPoint(DocumentationOptions documentationOptions)
        {
            var path = documentationOptions.OpenApiJsonUrl;
            
            if (path.StartsWith('/'))
            {
                path = path.TrimStart('/');
            }

            path = _baseDocumentationPath + path;

            var configuration = CreateEndPointMethodConfiguration(path);

            return new OpenApiJsonEndPointMethodHandler(_endPointServices, configuration, _openApiGenerationService);
        }
        
        private IEndPointMethodHandler RedirectEndPoint(DocumentationOptions documentationOptions)
        {
            var configuration = CreateEndPointMethodConfiguration(_basePath);

            return new RedirectEndPointHandler(_baseDocumentationPath + "index.html", configuration, _endPointServices);
        }

        private static EndPointMethodConfiguration CreateEndPointMethodConfiguration(string path)
        {
            var routeInfo = new RpcRouteInformation
            {
                HasBody = false,
                Method = HttpMethods.Get,
                RouteBasePath = path,
                RouteTemplate = path,
                Tokens = Array.Empty<IRpcRouteToken>()
            };

            var invokeFunc = new MethodInvokeInformation
            {
                MethodInvokeDelegate = context => null
            };

            var configuration = new EndPointMethodConfiguration(routeInfo, context => null, invokeFunc, typeof(void));
            return configuration;
        }


    }
}
