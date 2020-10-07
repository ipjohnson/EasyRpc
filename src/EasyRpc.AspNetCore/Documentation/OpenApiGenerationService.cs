using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Xml.Linq;
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
        void Configure(IInternalApiConfiguration apiInformation, DocumentationOptions documentationOptions,
            IReadOnlyList<IEndPointMethodHandler> endPointMethodHandlersList);

        Task Execute(HttpContext httpContext, RequestDelegate next);
    }

    public class OpenApiGenerationService : IOpenApiGenerationService
    {
        private IReadOnlyList<IEndPointMethodHandler> _endPointMethodHandlersList;
        private IInternalApiConfiguration _apiInformation;
        private readonly IOpenApiSchemaGenerator _apiSchemaGenerator;
        private readonly IContentSerializationService _contentSerializationService;
        private readonly IErrorResultTypeCreator _errorResultTypeCreator;
        private readonly IXmlDocProvider _xmlDocProvider;
        private ExposeConfigurations _exposeConfiguration;
        private DocumentationOptions _documentationOptions;
        private byte[] _cachedV3;

        public OpenApiGenerationService(IOpenApiSchemaGenerator apiSchemaGenerator,
            IConfigurationManager configurationManager,
            IContentSerializationService contentSerializationService,
            IErrorResultTypeCreator errorResultTypeCreator,
            IXmlDocProvider xmlDocProvider)
        {
            _apiSchemaGenerator = apiSchemaGenerator;
            _contentSerializationService = contentSerializationService;
            _errorResultTypeCreator = errorResultTypeCreator;
            _xmlDocProvider = xmlDocProvider;
        }

        public void Configure(IInternalApiConfiguration apiInformation, DocumentationOptions documentationOptions,
            IReadOnlyList<IEndPointMethodHandler> endPointMethodHandlersList)
        {
            _apiInformation = apiInformation;
            _endPointMethodHandlersList = endPointMethodHandlersList;
            _exposeConfiguration =
                apiInformation.AppServices.GetService<IConfigurationManager>().GetConfiguration<ExposeConfigurations>();
            _documentationOptions = documentationOptions;
            _apiSchemaGenerator.Configure(documentationOptions);
        }

        public Task Execute(HttpContext httpContext, RequestDelegate next)
        {
            httpContext.Request.Headers.TryGetValue("Accept-Encoding", out var encodings);

            var brCompress = encodings.ToString().Contains("br");

            bool isVersion3 = !(httpContext.Request.Query.TryGetValue("OpenApi", out var openApiVersion) &&
                                int.TryParse(openApiVersion, out var versionNumber) &&
                                versionNumber == 2);

            if (isVersion3 && _cachedV3 != null && brCompress)
            {
                httpContext.Response.Headers.TryAdd("Content-Encoding", "br");

                return httpContext.Response.Body.WriteAsync(_cachedV3, 0, _cachedV3.Length);
            }

            return GenerateAndReturnDocument(httpContext, next, isVersion3, brCompress);
        }

        private async Task GenerateAndReturnDocument(HttpContext httpContext, RequestDelegate next, bool isVersion3,
            bool brCompress)
        {
            httpContext.Response.ContentType = "application/json";

            var document = await GenerateDocument(httpContext);

            string outputString;

            if (isVersion3)
            {
                outputString = document.SerializeAsJson(OpenApiSpecVersion.OpenApi3_0);

                byte[] outputBytes;

                if (brCompress)
                {
                    using var memoryStream = new MemoryStream();
                    using var brStream = new BrotliStream(memoryStream, CompressionLevel.Optimal);
                    using var streamWriter = new StreamWriter(brStream);

                    streamWriter.Write(outputString);

                    streamWriter.Flush();
                    brStream.Flush();

                    _cachedV3 = outputBytes = memoryStream.ToArray();

                    httpContext.Response.Headers.TryAdd("Content-Encoding", "br");
                }
                else
                {
                    using var memoryStream = new MemoryStream();
                    using var streamWriter = new StreamWriter(memoryStream);

                    streamWriter.Write(outputString);
                    streamWriter.Flush();

                    outputBytes = memoryStream.ToArray();
                }

                await httpContext.Response.Body.WriteAsync(outputBytes, 0, outputBytes.Length);
            }
            else
            {
                outputString = document.SerializeAsJson(OpenApiSpecVersion.OpenApi2_0);

                await httpContext.Response.WriteAsync(outputString);
            }
        }


        protected virtual Task<OpenApiDocument> GenerateDocument(HttpContext context)
        {
            var document = new OpenApiDocument
            {
                Info = new OpenApiInfo
                {
                    Version = GetVersion(),
                    Title = GetTitle()
                }
            };

            ProcessEndPoints(context, document, _endPointMethodHandlersList);

            _apiSchemaGenerator.PopulateSchemaComponent(document);

            foreach (var documentFilter in _documentationOptions.DocumentFilters)
            {
                documentFilter.Apply(new DocumentFilterContext(document, context));
            }

            return Task.FromResult(document);
        }

        private string GetVersion()
        {
            var versionString = _documentationOptions.Version;

            if (string.IsNullOrEmpty(versionString))
            {
                var assemblyName = Assembly.GetEntryAssembly()?.GetName();

                if (assemblyName != null)
                {
                    versionString = _documentationOptions.VersionFormat(assemblyName.Version);
                }
            }

            return versionString;
        }

        private string GetTitle()
        {
            return _documentationOptions.Title ?? Assembly.GetEntryAssembly()?.GetName().Name;
        }

        private void ProcessEndPoints(HttpContext context, OpenApiDocument document, IReadOnlyList<IEndPointMethodHandler> endPointMethodHandlersList)
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
                    var apiOperation = GenerateApiOperation(document, endPointHandler.Value);

                    var method = GetOperationTypeFromHttpMethod(endPointHandler.Value.HttpMethod);

                    var filterContext = new OperationFilterContext(context, endPointHandler.Value.Configuration, apiOperation, document);

                    foreach (var operationFilter in _documentationOptions.OperationFilters)
                    {
                        operationFilter.Apply(filterContext);
                    }

                    pathItem.Operations.Add(method, apiOperation);
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

        private OpenApiOperation GenerateApiOperation(OpenApiDocument document,
            IEndPointMethodHandler endPointMethodHandler)
        {
            var methodInfo = endPointMethodHandler.Configuration.InvokeInformation.MethodToInvoke;

            XElement element = null;

            if (methodInfo != null)
            {
                element = _xmlDocProvider.GetMethodDocumentation(methodInfo);
            }

            var operationId = endPointMethodHandler.RouteInformation.RouteBasePath.Replace("/", "") + "-" +
                              endPointMethodHandler.HttpMethod;

            var operation = new OpenApiOperation
            {
                Summary = element.GetSummary(),
                Tags = new List<OpenApiTag>(),
                Parameters = GenerateParameters(endPointMethodHandler, element),
                OperationId = operationId
            };

            foreach (var tag in element.GetTags())
            {
                operation.Tags.Add(new OpenApiTag { Name = tag });
            }

            if (methodInfo != null && methodInfo.DeclaringType != null)
            {
                var parentElement = _xmlDocProvider.GetTypeDocumentation(methodInfo.DeclaringType);

                if (parentElement != null)
                {
                    foreach (var tag in parentElement.GetTags())
                    {
                        operation.Tags.Add(new OpenApiTag { Name = tag });
                    }
                }
            }

            if (operation.Tags.Count == 0 &&
                _documentationOptions.AutoTag &&
                methodInfo?.DeclaringType != null)
            {
                var tagName = methodInfo.DeclaringType.Name;

                operation.Tags.Add(new OpenApiTag { Name = tagName });

                if (document.Tags.All(tag => tag.Name != tagName))
                {
                    var parentElement = _xmlDocProvider.GetTypeDocumentation(methodInfo.DeclaringType);

                    var newTag = new OpenApiTag
                    {
                        Name = tagName,
                        Description = parentElement.GetSummary()
                    };

                    document.Tags.Add(newTag);
                }
            }

            if (endPointMethodHandler.RouteInformation.HasBody)
            {
                operation.RequestBody = GenerateRequestBody(endPointMethodHandler, element);
            }

            operation.Responses = GenerateResponses(endPointMethodHandler, element);

            return operation;
        }


        private OpenApiResponses GenerateResponses(IEndPointMethodHandler endPointMethodHandler, XElement element)
        {
            var responses = new OpenApiResponses();
            var hasContent = false;

            if (endPointMethodHandler.Configuration.ReturnType != typeof(void) &&
                endPointMethodHandler.Configuration.ReturnType != typeof(Task) &&
                endPointMethodHandler.Configuration.ReturnType != typeof(ValueTask))
            {
                hasContent = true;

                GenerateSuccessResponse(endPointMethodHandler, responses);
            }

            GenerateNoContentResponse(endPointMethodHandler, responses, !hasContent);

            GenerateErrorResponse(endPointMethodHandler, responses);

            return responses;
        }

        private void GenerateEmptySuccessResponse(IEndPointMethodHandler endPointMethodHandler, OpenApiResponses responses)
        {
            var successStatusCode = endPointMethodHandler.Configuration.SuccessStatusCode.ToString();
            var contentDictionary = new Dictionary<string, OpenApiMediaType>();
            var response = new OpenApiResponse { Content = contentDictionary, Description = "Success" };

            responses.Add(successStatusCode, response);
        }

        private void GenerateErrorResponse(IEndPointMethodHandler endPointMethodHandler, OpenApiResponses responses)
        {
            if (!responses.ContainsKey("500"))
            {
                var response = new OpenApiResponse { Description = "Internal Server Error" };

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

        private void GenerateNoContentResponse(IEndPointMethodHandler endPointMethodHandler, OpenApiResponses responses, bool success)
        {
            if (endPointMethodHandler.RouteInformation.HasBody)
            {
                if (!responses.ContainsKey("204"))
                {
                    var response = new OpenApiResponse { Description = "No content" };

                    if (success)
                    {
                        response.Description += " - successful";
                    }

                    responses.Add("204", response);
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

            var returnType = endPointMethodHandler.Configuration.ReturnType;

            if (returnType.IsConstructedGenericType &&
                (returnType.GetGenericTypeDefinition() == typeof(Task<>) ||
                 returnType.GetGenericTypeDefinition() == typeof(ValueTask<>)))
            {
                returnType = returnType.GenericTypeArguments[0];
            }

            if (endPointMethodHandler.Configuration.WrappedType != null)
            {
                responseSchema = new OpenApiSchema
                {
                    Properties = new Dictionary<string, OpenApiSchema>
                    {
                        { "result", _apiSchemaGenerator.GetSchemaType(returnType) }
                    }
                };
            }
            else
            {
                responseSchema = _apiSchemaGenerator.GetSchemaType(returnType);
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
                contentDictionary[endPointMethodHandler.Configuration.RawContentType] = new OpenApiMediaType
                {
                    Schema = responseSchema
                };
            }

            responses.Add(successStatusCode, response);
        }

        private OpenApiRequestBody GenerateRequestBody(IEndPointMethodHandler endPointMethodHandler, XElement element)
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

                foreach (var schemaProperty in schema.Properties)
                {
                    schemaProperty.Value.Description = element.GetParameterSummary(schemaProperty.Key);
                }

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

                schema.Xml = new OpenApiXml
                {
                    Name = "args"
                };

                foreach (var schemaProperty in schema.Properties)
                {
                    schemaProperty.Value.Description = element.GetParameterSummary(schemaProperty.Key);
                }

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

        private IList<OpenApiParameter> GenerateParameters(IEndPointMethodHandler endPointMethodHandler,
            XElement element)
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
                    Description = element.GetParameterSummary(configurationParameter.Name),
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
