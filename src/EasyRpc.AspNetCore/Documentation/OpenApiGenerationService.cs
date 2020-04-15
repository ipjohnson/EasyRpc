using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using EasyRpc.AspNetCore.CodeGeneration;
using EasyRpc.AspNetCore.Configuration;
using EasyRpc.AspNetCore.EndPoints;
using EasyRpc.AspNetCore.Serializers;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi;
using Microsoft.OpenApi.Extensions;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Writers;

namespace EasyRpc.AspNetCore.Documentation
{
    public interface IOpenApiGenerationService
    {
        void Configure(IInternalApiConfiguration apiInformation, IReadOnlyList<IEndPointMethodHandler> endPointMethodHandlersList);

        Task Execute(HttpContext httpContext, RequestDelegate next);
    }

    public class OpenApiGenerationService : IOpenApiGenerationService
    {
        private IReadOnlyList<IEndPointMethodHandler> _endPointMethodHandlersList;
        private IInternalApiConfiguration _apiInformation;
        private IOpenApiSchemaGenerator _apiSchemaGenerator;
        private IContentSerializationService _contentSerializationService;
        private IErrorResultTypeCreator _errorResultTypeCreator;
        private ExposeConfigurations _exposeConfiguration;

        public OpenApiGenerationService(IOpenApiSchemaGenerator apiSchemaGenerator,
            IConfigurationManager configurationManager,
            IContentSerializationService contentSerializationService, 
            IErrorResultTypeCreator errorResultTypeCreator)
        {
            _apiSchemaGenerator = apiSchemaGenerator;
            _contentSerializationService = contentSerializationService;
            _errorResultTypeCreator = errorResultTypeCreator;
        }

        public void Configure(IInternalApiConfiguration apiInformation, IReadOnlyList<IEndPointMethodHandler> endPointMethodHandlersList)
        {
            _apiInformation = apiInformation;
            _endPointMethodHandlersList = endPointMethodHandlersList;
            _exposeConfiguration = apiInformation.AppServices.GetService<IConfigurationManager>().GetConfiguration<ExposeConfigurations>();
        }

        public async Task Execute(HttpContext httpContext, RequestDelegate next)
        {
            httpContext.Response.ContentType = "application/json";

            var document = await GenerateDocument();

            var output = document.SerializeAsJson(OpenApiSpecVersion.OpenApi3_0);

            await httpContext.Response.WriteAsync(output);
        }

        protected virtual async Task<OpenApiDocument> GenerateDocument()
        {
            var document = new OpenApiDocument
            {
                Info = new OpenApiInfo
                {
                    Version = "v1",
                    Title = GetTitle()
                }
            };

            ProcessEndPoints(document, _endPointMethodHandlersList);

            _apiSchemaGenerator.PopulateSchemaComponent(document);

            return document;
        }

        private string GetTitle()
        {
            return Assembly.GetEntryAssembly().GetName().Name;
        }

        private void ProcessEndPoints(OpenApiDocument document, IReadOnlyList<IEndPointMethodHandler> endPointMethodHandlersList)
        {
            document.Paths = new OpenApiPaths();

            var groupedMethods = GroupMethodsByTemplate(endPointMethodHandlersList);

            foreach (var groupedMethod in groupedMethods)
            {
                var pathItem = new OpenApiPathItem
                {
                    Operations = new Dictionary<OperationType, OpenApiOperation>()
                };

                foreach (var endPointHandler in groupedMethod.Value)
                {
                    var apiOperation = GenerateApiOperation(endPointHandler.Value);

                    pathItem.Operations.Add(GetOperationTypeFromHttpMethod(endPointHandler.Value.HttpMethod), apiOperation);
                }

                document.Paths.Add(groupedMethod.Key, pathItem);
            }
        }

        private OperationType GetOperationTypeFromHttpMethod(string httpMethod)
        {
            if (httpMethod == HttpMethods.Get)
            {
                return OperationType.Get;
            }

            if (httpMethod == HttpMethods.Post)
            {
                return OperationType.Post;
            }

            if (httpMethod == HttpMethods.Head)
            {
                return OperationType.Head;
            }

            if (httpMethod == HttpMethods.Patch)
            {
                return OperationType.Patch;
            }

            if (httpMethod == HttpMethods.Put)
            {
                return OperationType.Put;
            }

            if (httpMethod == HttpMethods.Delete)
            {
                return OperationType.Delete;
            }

            if (httpMethod == HttpMethods.Options)
            {
                return OperationType.Options;
            }

            return OperationType.Post;
        }

        private Dictionary<string, Dictionary<string, IEndPointMethodHandler>> GroupMethodsByTemplate(IReadOnlyList<IEndPointMethodHandler> endPointMethodHandlersList)
        {
            var groupings = new Dictionary<string, Dictionary<string, IEndPointMethodHandler>>();

            foreach (var handler in endPointMethodHandlersList)
            {
                if (!groupings.TryGetValue(handler.RouteInformation.RouteTemplate, out var methodGroup))
                {
                    methodGroup = new Dictionary<string, IEndPointMethodHandler>();

                    groupings[handler.RouteInformation.RouteTemplate] = methodGroup;
                }

                methodGroup[handler.HttpMethod] = handler;
            }

            return groupings;
        }

        private OpenApiOperation GenerateApiOperation(IEndPointMethodHandler endPointMethodHandler)
        {
            var operation = new OpenApiOperation
            {
                Description = "test",
                Parameters = GenerateParameters(endPointMethodHandler)
            };

            if (endPointMethodHandler.RouteInformation.HasBody)
            {
                operation.RequestBody = GenerateRequestBody(endPointMethodHandler);
            }

            operation.Responses = GenerateResponses(endPointMethodHandler);

            return operation;
        }

        private OpenApiResponses GenerateResponses(IEndPointMethodHandler endPointMethodHandler)
        {
            var responses = new OpenApiResponses();

            GenerateSuccessResponse(endPointMethodHandler, responses);

            GenerateNoContentResponse(endPointMethodHandler, responses);

            GenerateErrorResponse(endPointMethodHandler, responses);

            return responses;
        }

        private void GenerateErrorResponse(IEndPointMethodHandler endPointMethodHandler, OpenApiResponses responses)
        {
            if (!responses.ContainsKey("500"))
            {
                var response = new OpenApiResponse {Description = "Internal Server Error"};

                var contentDictionary = response.Content = new Dictionary<string, OpenApiMediaType>();
                var responseSchema = _apiSchemaGenerator.GetSchemaType(_errorResultTypeCreator.GenerateErrorType());

                foreach (var supportedContentType in _contentSerializationService.SupportedContentTypes)
                {
                    contentDictionary[supportedContentType] = new OpenApiMediaType
                    {
                        Schema = responseSchema
                    };
                }

                responses.Add("500", response);
            }
        }

        private void GenerateNoContentResponse(IEndPointMethodHandler endPointMethodHandler, OpenApiResponses responses)
        {
            if (endPointMethodHandler.RouteInformation.HasBody)
            {
                if (!responses.ContainsKey("204"))
                {
                    responses.Add("204", new OpenApiResponse { Description = "No content" });
                }
            }
            else
            {
                if (!responses.ContainsKey("404"))
                {
                    responses.Add("404", new OpenApiResponse { Description = "No resource found" });
                }
            }
        }

        private void GenerateSuccessResponse(IEndPointMethodHandler endPointMethodHandler, OpenApiResponses responses)
        {
            var successStatusCode = endPointMethodHandler.Configuration.SuccessStatusCode.ToString();
            var contentDictionary = new Dictionary<string, OpenApiMediaType>();
            var response = new OpenApiResponse { Content = contentDictionary, Description = "Success" };
            OpenApiSchema responseSchema = null;

            if (string.IsNullOrEmpty(endPointMethodHandler.Configuration.RawContentType) &&
                _exposeConfiguration.TypeWrapSelector(endPointMethodHandler.Configuration.ReturnType))
            {
                responseSchema = new OpenApiSchema
                {
                    Properties = new Dictionary<string, OpenApiSchema>
                    {
                        { "result", _apiSchemaGenerator.GetSchemaType(endPointMethodHandler.Configuration.ReturnType) }
                    }
                };
            }
            else
            {
                responseSchema = _apiSchemaGenerator.GetSchemaType(endPointMethodHandler.Configuration.ReturnType);
            }

            if (string.IsNullOrEmpty(endPointMethodHandler.Configuration.RawContentType))
            {
                foreach (var supportedContentType in _contentSerializationService.SupportedContentTypes)
                {
                    contentDictionary[supportedContentType] = new OpenApiMediaType
                    {
                        Schema = responseSchema
                    };
                }
            }
            else
            {
                contentDictionary[endPointMethodHandler.Configuration.RawContentType] = new OpenApiMediaType();
            }

            responses.Add(successStatusCode, response);
        }

        private OpenApiRequestBody GenerateRequestBody(IEndPointMethodHandler endPointMethodHandler)
        {
            var requestParameter =
                endPointMethodHandler.Configuration.Parameters.FirstOrDefault(p =>
                    p.ParameterSource == EndPointMethodParameterSource.PostBody);

            if (requestParameter != null)
            {
                var request = new OpenApiRequestBody
                {
                    Content = new Dictionary<string, OpenApiMediaType>()
                };

                var schema = _apiSchemaGenerator.GetSchemaType(requestParameter.ParamType);

                foreach (var contentType in _contentSerializationService.SupportedContentTypes)
                {
                    request.Content[contentType] = new OpenApiMediaType
                    {
                        Schema = schema
                    };
                }

                return request;
            }

            var postParameterList = endPointMethodHandler.Configuration.Parameters.Where(p =>
                p.ParameterSource == EndPointMethodParameterSource.PostParameter).ToList();

            if (postParameterList.Count > 0)
            {
                var request = new OpenApiRequestBody
                {
                    Content = new Dictionary<string, OpenApiMediaType>()
                };

                var schema = GeneratePostParameterSchema(postParameterList);

                foreach (var contentType in _contentSerializationService.SupportedContentTypes)
                {
                    request.Content[contentType] = new OpenApiMediaType
                    {
                        Schema = schema
                    };
                }

                return request;
            }

            return null;
        }

        private OpenApiSchema GeneratePostParameterSchema(List<RpcParameterInfo> postParameterList)
        {
            var schema = new OpenApiSchema();

            schema.Type = "object";
            var propertiesDictionary = schema.Properties = new Dictionary<string, OpenApiSchema>();

            foreach (var rpcParameterInfo in postParameterList)
            {
                var propertySchema = _apiSchemaGenerator.GetSchemaType(rpcParameterInfo.ParamType);

                propertiesDictionary[rpcParameterInfo.Name] = propertySchema;
            }

            return schema;
        }

        private IList<OpenApiParameter> GenerateParameters(IEndPointMethodHandler endPointMethodHandler)
        {
            var parameterList = new List<OpenApiParameter>();

            var urlParameterEnumerable = endPointMethodHandler.Configuration.Parameters.Where(p =>
                p.ParameterSource == EndPointMethodParameterSource.PathParameter ||
                p.ParameterSource == EndPointMethodParameterSource.QueryStringParameter);

            foreach (var configurationParameter in urlParameterEnumerable)
            {
                var apiParameter = new OpenApiParameter
                {
                    Name = configurationParameter.Name,
                    Schema = _apiSchemaGenerator.GetSchemaType(configurationParameter.ParamType)
                };

                switch (configurationParameter.ParameterSource)
                {
                    case EndPointMethodParameterSource.PathParameter:
                        apiParameter.In = ParameterLocation.Path;
                        break;
                    case EndPointMethodParameterSource.QueryStringParameter:
                        apiParameter.In = ParameterLocation.Query;
                        break;
                }

                parameterList.Add(apiParameter);
            }

            return parameterList;
        }
    }
}
