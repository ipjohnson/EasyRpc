using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using EasyRpc.AspNetCore.Content;
using EasyRpc.AspNetCore.Messages;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace EasyRpc.AspNetCore.Middleware
{

    public interface IRpcMessageProcessor
    {
        EndPointConfiguration Configure(IApiConfigurationProvider configuration, string route);

        Task ProcessRequest(HttpContext context);
    }

    public class RpcMessageProcessor : IRpcMessageProcessor
    {
        private static readonly object[] NoParamsArray = new object[0];

        private readonly IInstanceActivator _activator;
        private readonly IOptions<RpcServiceConfiguration> _configuration;
        private readonly IContentEncodingProvider _contentEncodingProvider;
        private readonly IContentSerializerProvider _contentSerializerProvider;
        private readonly IExposeMethodInformationCacheManager _cacheManager;
        private readonly bool _debugLogging;
        private readonly ILogger<RpcMessageProcessor> _logger;
        private string _route;
        private readonly IContentSerializer _defaultSerializer;

        public RpcMessageProcessor(IOptions<RpcServiceConfiguration> configuration,
            IContentEncodingProvider contentEncodingProvider,
            IContentSerializerProvider contentSerializerProvider,
            IExposeMethodInformationCacheManager cacheManager,
            IInstanceActivator activator,
            ILogger<RpcMessageProcessor> logger = null)
        {
            _configuration = configuration;
            _contentEncodingProvider = contentEncodingProvider;
            _contentSerializerProvider = contentSerializerProvider;
            _cacheManager = cacheManager;
            _activator = activator;
            _logger = logger;
            _debugLogging = configuration.Value.DebugLogging;
            _defaultSerializer = _contentSerializerProvider.DefaultSerializer;
        }

        public EndPointConfiguration Configure(IApiConfigurationProvider configuration, string route)
        {
            _route = route;
            var exposedMethodInformations = new Dictionary<string, IExposedMethodInformation>();
            foreach (var exposedMethod in configuration.GetExposedMethods())
            {
                foreach (var name in exposedMethod.RouteNames)
                {
                    exposedMethodInformations[name + "*" + exposedMethod.MethodName] = exposedMethod;
                }
            }

            var currentApiInfo = configuration.GetCurrentApiInformation();

            var endPoint =
                new EndPointConfiguration(route, exposedMethodInformations, currentApiInfo.EnableDocumentation, currentApiInfo.DocumentationConfiguration);

            _cacheManager.Configure(endPoint);

            _contentSerializerProvider.Configure(_cacheManager);

            return endPoint;
        }

        public async Task ProcessRequest(HttpContext context)
        {
            RpcRequestPackage requestPackage = null;

            if (_debugLogging)
            {
                _logger?.LogInformation(new EventId(10), $"Processing json-rpc request for path {context.Request.Path}");
            }

            context.Response.StatusCode = StatusCodes.Status200OK;

            var path = context.Request.Path.Value.Substring(_route.Length).TrimEnd('/');
            Stream body = context.Request.Body;

            try
            {
                if (_configuration.Value.SupportRequestCompression &&
                    context.Request.Headers.TryGetValue("Content-Encoding", out var encoding))
                {
                    var encoder = _contentEncodingProvider.GetContentEncoder(encoding.ToString());

                    if (encoder == null)
                    {
                        // write error
                    }
                    else
                    {
                        body = encoder.DecodeStream(body);
                    }
                }

                requestPackage = await DeserializeStream(context, body, path);
            }
            catch (Exception exp)
            {
                await ProcessRequestSerizliationErrorHandler(context, _defaultSerializer, exp);
            }
            finally
            {
                if (body != context.Request.Body)
                {
                    await body.DisposeAsync();
                }
            }

            if (requestPackage != null)
            {
                if (requestPackage.IsBulk)
                {
                    await ProcessBulkRequest(context, requestPackage, path);
                }

                var message = requestPackage.Requests.First();

                if (message.Parameters != null)
                {
                    await ProcessRequest(context, requestPackage.Serializer, requestPackage.Requests.First(), path);
                }
                else
                {
                    await WriteErrorMessage(context, _defaultSerializer,
                        new ErrorResponseMessage(JsonRpcErrorCode.MethodNotFound,
                            "Count not find method " + message.Method));
                }
            }
            else
            {
                await WriteErrorMessage(context, _defaultSerializer,
                    new ErrorResponseMessage(JsonRpcErrorCode.InvalidRequest, "Could not parse request"));
            }
        }

        private Task ProcessRequestSerizliationErrorHandler(HttpContext context, IContentSerializer serializer, Exception exp)
        {
            _logger?.LogError(EventIdCode.DeserializeException, exp,
                "Exception thrown while deserializing request package: " + exp.Message);

            return WriteErrorMessage(context, serializer,
                new ErrorResponseMessage(JsonRpcErrorCode.InvalidRequest,
                    "Could not parse request: " + exp.Message));
        }

        private async Task<RpcRequestPackage> DeserializeStream(HttpContext context, Stream stream, string path)
        {
            var serializer = _contentSerializerProvider.GetSerializer(context);

            var package = await serializer.DeserializeRequestPackage(stream, path, context);

            package.Serializer = serializer;

            return package;
        }

        private async Task ProcessBulkRequest(HttpContext context, RpcRequestPackage requestPackage, string s)
        {
            var path = context.Request.Path.Value.Substring(_route.Length).TrimEnd('/');

            var returnList = new List<ResponseMessage>();

            foreach (var request in requestPackage.Requests)
            {
                returnList.Add(await ProcessIndividualRequest(context, context.RequestServices, path, request));
            }

            try
            {
                await SerializeToResponseBody(context, requestPackage.Serializer, returnList, _configuration.Value.SupportResponseCompression);
            }
            catch (Exception exp)
            {
                _logger?.LogError(EventIdCode.DeserializeException, exp, "Exception thrown while serializing bulk output: " + exp.Message);

                await WriteErrorMessage(context, requestPackage.Serializer,
                    new ErrorResponseMessage(JsonRpcErrorCode.InternalServerError, "Internal Server Error"));
            }
        }

        private async Task SerializeToResponseBody(HttpContext context, IContentSerializer serializer, object values,
            bool canCompress)
        {
            Stream responseStream = context.Response.Body;

            if (canCompress)
            {
                if (context.Request.Headers.TryGetValue("Accept-Encoding", out var encoding))
                {
                    var encoder = _contentEncodingProvider.GetContentEncoder(encoding.ToString());

                    if (encoder != null)
                    {
                        context.Response.Headers["Content-Encoding"] = encoder.ContentEncoding;

                        responseStream = encoder.EncodeStream(responseStream);
                    }
                }
            }

            await SerializeToStream(context, serializer, values, responseStream);

            if (responseStream != context.Response.Body)
            {
                await responseStream.DisposeAsync();
            }
        }

        private Task SerializeToStream(HttpContext context, IContentSerializer serializer, object values, Stream stream)
        {
            context.Response.ContentType = serializer.ContentType;

            return serializer.SerializeResponse(stream, values, context);
        }

        private async Task ProcessRequest(HttpContext context, IContentSerializer serializer, RpcRequestMessage requestMessage, string path)
        {
            var response = await ProcessIndividualRequest(context, context.RequestServices, path, requestMessage);

            try
            {
                if (!ReferenceEquals(response, ResponseMessage.NoResponse))
                {
                    await SerializeToResponseBody(context, serializer, response, response.CanCompress);
                }
            }
            catch (Exception exp)
            {
                await ProcessRequestErrorHandler(context, serializer, requestMessage, exp);
            }
        }

        private Task ProcessRequestErrorHandler(HttpContext context, IContentSerializer serializer, RpcRequestMessage requestMessage, Exception exp)
        {
            _logger?.LogError(EventIdCode.SerializeException, exp,
                "Exception thrown while serializing response: " + exp.Message);

            var errorMessage = "Internal Server Error";

            if (_configuration.Value.ShowErrorMessage)
            {
                errorMessage += ": " + exp.Message;
            }

            return WriteErrorMessage(context, serializer,
                new ErrorResponseMessage(JsonRpcErrorCode.InternalServerError, errorMessage, requestMessage.Version, requestMessage.Id));
        }


        private Task<ResponseMessage> ProcessIndividualRequest(HttpContext context, IServiceProvider serviceProvider,
            string path, RpcRequestMessage requestMessage)
        {
            if (requestMessage.MethodInformation != null)
            {
                if (_debugLogging)
                {
                    _logger?.LogDebug($"Found method for {path} {requestMessage.Method}");
                }

                return ExecuteMethod(context, serviceProvider, requestMessage, requestMessage.MethodInformation);
            }

            _logger?.LogError($"No method {requestMessage.Method} found at {path}");

            return ReturnMethodNotFound(requestMessage.Version, requestMessage.Id);
        }

        private async Task<ResponseMessage> ExecuteMethod(HttpContext context, IServiceProvider serviceProvider,
            RpcRequestMessage requestMessage, IExposedMethodInformation exposedMethod)
        {
            CallExecutionContext callExecutionContext =
                new CallExecutionContext(context, exposedMethod.InstanceType, exposedMethod.MethodInfo, requestMessage);

            if (exposedMethod.MethodAuthorizations.Length > 0)
            {
                for (var i = 0; i < exposedMethod.MethodAuthorizations.Length; i++)
                {
                    if (!await exposedMethod.MethodAuthorizations[i].AsyncAuthorize(callExecutionContext))
                    {
                        if (_debugLogging)
                        {
                            _logger?.LogDebug($"Unauthorized access to {context.Request.Path} {requestMessage.Method}");
                        }

                        return ReturnUnauthorizedAccess(context, requestMessage.Version, requestMessage.Id);
                    }
                }
            }

            object newInstance = null;

            try
            {
                newInstance = exposedMethod.InstanceProvider(context, serviceProvider);

                callExecutionContext.Instance = newInstance;
            }
            catch (Exception exp)
            {
                _logger?.LogError(EventIdCode.ActivationException, exp, $"Exception thrown while creating instance {exposedMethod.InstanceType.Name} for {context.Request.Path} {requestMessage.Method} - " + exp.Message);

                // log error 
                return ReturnInternalServerError(requestMessage.Version, requestMessage.Id, $" Could not activate type {exposedMethod.InstanceType.FullName}\n{exp.Message}");
            }

            if (newInstance == null)
            {
                return ReturnInternalServerError(requestMessage.Version, requestMessage.Id, $"Could not locate type {exposedMethod.InstanceType.FullName}");
            }

            List<ICallFilter> filters = null;
            bool runFilters = false;
            var exposedFilters = exposedMethod.Filters;

            if (exposedFilters.Length > 0)
            {
                filters = new List<ICallFilter>();

                try
                {
                    for (var i = 0; i < exposedFilters.Length; i++)
                    {
                        filters.AddRange(exposedFilters[i](callExecutionContext));
                    }
                }
                catch (Exception exp)
                {
                    _logger?.LogError(EventIdCode.ActivationException, exp, $"Exception thrown while activating filters for {exposedMethod.InstanceType.Name} {context.Request.Path} {requestMessage.Method} - " + exp.Message);

                    return ReturnInternalServerError(requestMessage.Version, requestMessage.Id, "Could not activate filters");
                }

                runFilters = filters.Count > 0;
            }

            try
            {
                object[] parameterValues = requestMessage.Parameters ?? NoParamsArray;

                if (runFilters)
                {
                    callExecutionContext.Parameters = parameterValues;

                    for (var i = 0; i < filters.Count; i++)
                    {
                        if (callExecutionContext.ContinueCall &&
                            filters[i] is ICallExecuteFilter executeFilter)
                        {
                            await executeFilter.BeforeExecute(callExecutionContext);
                        }
                    }

                    parameterValues = callExecutionContext.Parameters;
                }

                ResponseMessage responseMessage;

                if (callExecutionContext.ContinueCall)
                {
                    responseMessage = await exposedMethod.InvokeMethod(newInstance, parameterValues, requestMessage.Version, requestMessage.Id);
                }
                else
                {
                    return callExecutionContext.ResponseMessage;
                }

                if (runFilters &&
                    callExecutionContext.ContinueCall)
                {
                    callExecutionContext.ResponseMessage = responseMessage;

                    for (var i = 0; i < filters.Count; i++)
                    {
                        if (callExecutionContext.ContinueCall &&
                            filters[i] is ICallExecuteFilter executeFilter)
                        {
                            await executeFilter.AfterExecute(callExecutionContext);
                        }
                    }

                    return callExecutionContext.ResponseMessage;
                }

                return responseMessage;
            }
            catch (Exception exp)
            {
                return await ExecuteMethodErrorHandler(context, requestMessage, exp, runFilters, filters, callExecutionContext);
            }
        }

        private async Task<ResponseMessage> ExecuteMethodErrorHandler(HttpContext context, RpcRequestMessage requestMessage, Exception exp,
            bool runFilters, List<ICallFilter> filters, CallExecutionContext callExecutionContext)
        {
            if (runFilters)
            {
                foreach (var callFilter in filters)
                {
                    if (callFilter is ICallExceptionFilter exceptionFilter)
                    {
                        try
                        {
                            await exceptionFilter.HandleException(callExecutionContext, exp);
                        }
                        catch (Exception e)
                        {
                            _logger?.LogError(EventIdCode.ExecutionFilterException, e,
                                "Exception thrown while invoking ICallExceptionFilter");
                        }
                    }
                }
            }

            _logger?.LogError(EventIdCode.ExecutionException, exp,
                $"Exception thrown while processing {context.Request.Path} {requestMessage.Method} - " + exp.Message);

            return ReturnInternalServerError(requestMessage.Version, requestMessage.Id,
                $"Executing {context.Request.Path} {requestMessage.Method} {exp.Message}");
        }

        private Task WriteErrorMessage(HttpContext context, IContentSerializer serializer, ErrorResponseMessage errorResponseMessage)
        {
            context.Response.ContentType = serializer.ContentType;

            return serializer.SerializeResponse(context.Response.Body, errorResponseMessage, context);
        }

        private Task<ResponseMessage> ReturnMethodNotFound(string version, string id)
        {
            return
                Task.FromResult<ResponseMessage>(new ErrorResponseMessage(JsonRpcErrorCode.MethodNotFound,
                    "Method not found", version, id));
        }

        private ResponseMessage ReturnInternalServerError(string version, string id, string expMessage)
        {
            var errorMessage = "Internal Server Error";

            if (_configuration.Value.ShowErrorMessage)
            {
                errorMessage += " " + expMessage;
            }

            return new ErrorResponseMessage(JsonRpcErrorCode.InternalServerError, errorMessage, version, id);
        }

        private ResponseMessage ReturnUnauthorizedAccess(HttpContext context, string version, string id)
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;

            return new ErrorResponseMessage(JsonRpcErrorCode.UnauthorizedAccess, "No access to this method", version);
        }
    }
}
