using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using EasyRpc.AspNetCore.Messages;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;

namespace EasyRpc.AspNetCore.Middleware
{

    public interface IJsonRpcMessageProcessor
    {
        void Configure(IApiConfigurationProvider configuration, string route);

        Task ProcessRequest(HttpContext context);
    }

    public class JsonRpcMessageProcessor : IJsonRpcMessageProcessor
    {
        private static readonly object[] NoParamsArray = new object[0];

        private readonly ConcurrentDictionary<string, IExposedMethodCache> _methodCache;
        private readonly JsonSerializer _serializer;
        private readonly ConcurrentDictionary<string, ExposedMethodInformation> _exposedMethodInformations;
        private readonly INamedParameterToArrayDelegateProvider _namedParameterToArrayDelegateProvider;
        private readonly IOrderedParameterToArrayDelegateProvider _orderedParameterToArrayDelegateProvider;
        private readonly IArrayMethodInvokerBuilder _invokerBuilder;
        private readonly IOptions<RpcServiceConfiguration> _configuration;
        private readonly bool _debugLogging;
        private readonly ILogger<JsonRpcMessageProcessor> _logger;
        private string _route;

        public JsonRpcMessageProcessor(IOptions<RpcServiceConfiguration> configuration,
            IJsonSerializerProvider provider,
            INamedParameterToArrayDelegateProvider namedParameterToArrayDelegateProvider,
            IOrderedParameterToArrayDelegateProvider orderedParameterToArrayDelegateProvider,
            IArrayMethodInvokerBuilder invokerBuilder,
            ILogger<JsonRpcMessageProcessor> logger = null)
        {
            _namedParameterToArrayDelegateProvider = namedParameterToArrayDelegateProvider;
            _configuration = configuration;
            _orderedParameterToArrayDelegateProvider = orderedParameterToArrayDelegateProvider;
            _invokerBuilder = invokerBuilder;
            _logger = logger;
            _serializer = provider.ProvideSerializer();
            _methodCache = new ConcurrentDictionary<string, IExposedMethodCache>();
            _exposedMethodInformations = new ConcurrentDictionary<string, ExposedMethodInformation>();
            _debugLogging = configuration.Value.DebugLogging;
        }

        public void Configure(IApiConfigurationProvider configuration, string route)
        {
            _route = route;

            foreach (var exposedMethod in configuration.GetExposedMethods())
            {
                foreach (var name in exposedMethod.RouteNames)
                {
                    _exposedMethodInformations[name + "|" + exposedMethod.MethodName] = exposedMethod;
                }
            }
        }

        public Task ProcessRequest(HttpContext context)
        {
            RequestPackage requestPackage = null;

            if (_debugLogging)
            {
                _logger?.LogInformation(new EventId(10), $"Processing json-rpc request for path {context.Request.Path}");
            }

            context.Response.ContentType = "application/json";

            try
            {
                if (_configuration.Value.SupportRequestCompression &&
                    context.Request.Headers["Content-Encoding"].Contains("gzip"))
                {
                    using (var gzipStream = new GZipStream(context.Request.Body, CompressionMode.Decompress))
                    {
                        requestPackage = DeserializeStream(gzipStream);
                    }
                }
                else
                {
                    requestPackage = DeserializeStream(context.Request.Body);
                }
            }
            catch (Exception exp)
            {
                return ProcessRequestSerizliationErrorHandler(context, exp);
            }

            if (requestPackage != null)
            {
                if (!requestPackage.IsBulk)
                {
                    return ProcessRequest(context, requestPackage.Requests.First());
                }

                return ProcessBulkRequest(context, requestPackage);
            }

            WriteErrorMessage(context,
                 new ErrorResponseMessage("2.0", "", JsonRpcErrorCode.InvalidRequest, "Could not parse request"));

            return Task.CompletedTask;
        }

        private Task ProcessRequestSerizliationErrorHandler(HttpContext context, Exception exp)
        {
            _logger?.LogError(EventIdCode.DeserializeException, exp,
                "Exception thrown while deserializing request package: " + exp.Message);

            WriteErrorMessage(context,
                new ErrorResponseMessage("2.0", "", JsonRpcErrorCode.InvalidRequest,
                    "Could not parse request: " + exp.Message));

            return Task.CompletedTask;
        }

        private RequestPackage DeserializeStream(Stream gzipStream)
        {
            using (var streamReader = new StreamReader(gzipStream))
            {
                using (var jsonReader = new JsonTextReader(streamReader))
                {
                    return _serializer.Deserialize<RequestPackage>(jsonReader);
                }
            }
        }

        private async Task ProcessBulkRequest(HttpContext context, RequestPackage requestPackage)
        {
            var path = context.Request.Path.Value.Substring(_route.Length).TrimEnd('/');

            var returnListTask = new List<Task<ResponseMessage>>();

            foreach (var request in requestPackage.Requests)
            {
                var requestMessage = request;

                var newTask = Task.Run(
                    () => ProcessRequestMultiThreaded(context, path, requestMessage));

                returnListTask.Add(newTask);
            }

            var values = await Task.WhenAll(returnListTask);

            try
            {
                SerializeToResponseBody(context, values);
            }
            catch (Exception exp)
            {
                _logger?.LogError(EventIdCode.DeserializeException, exp, "Exception thrown while serializing bulk output: " + exp.Message);

                WriteErrorMessage(context,
                    new ErrorResponseMessage("2.0", "",
                        JsonRpcErrorCode.InternalServerError, "Internal Server Error"));
            }
        }

        private void SerializeToResponseBody(HttpContext context, object values)
        {
            if (_configuration.Value.SupportResponseCompression &&
                context.Request.Headers["Accept-Encoding"].Contains("gzip"))
            {
                context.Response.Headers["Content-Encoding"] = new StringValues("gzip");

                using (var gzipStream = new GZipStream(context.Response.Body, CompressionLevel.Fastest))
                {
                    SerializeToStream(values, gzipStream);
                }
            }
            else
            {
                SerializeToStream(values, context.Response.Body);
            }
        }

        private void SerializeToStream(object values, Stream gzipStream)
        {
            using (var responseStream = new StreamWriter(gzipStream))
            {
                using (var jsonStream = new JsonTextWriter(responseStream))
                {
                    _serializer.Serialize(jsonStream, values);
                }
            }
        }

        private async Task<ResponseMessage> ProcessRequestMultiThreaded(HttpContext context, string path, RequestMessage requestMessage)
        {
            return await ProcessIndividualRequest(context, context.RequestServices, path, requestMessage);
        }

        private async Task ProcessRequest(HttpContext context, RequestMessage requestMessage)
        {
            var path = context.Request.Path.Value.Substring(_route.Length).TrimEnd('/');

            var response = await ProcessIndividualRequest(context, context.RequestServices, path, requestMessage);

            try
            {
                SerializeToResponseBody(context, response);
            }
            catch (Exception exp)
            {
                ProcessRequestErrorHandler(context, requestMessage, exp);
            }
        }

        private void ProcessRequestErrorHandler(HttpContext context, RequestMessage requestMessage, Exception exp)
        {
            _logger?.LogError(EventIdCode.SerializeException, exp,
                "Exception thrown while serializing response: " + exp.Message);

            var errorMessage = "Internal Server Error";

            if (_configuration.Value.ShowErrorMessage)
            {
                errorMessage += ": " + exp.Message;
            }

            WriteErrorMessage(context,
                new ErrorResponseMessage(requestMessage.Version, requestMessage.Id,
                    JsonRpcErrorCode.InternalServerError, errorMessage));
        }


        private Task<ResponseMessage> ProcessIndividualRequest(HttpContext context, IServiceProvider serviceProvider,
            string path, RequestMessage requestMessage)
        {
            var methodKey = $"{path}|{requestMessage.Method}";

            if (!_methodCache.TryGetValue(methodKey, out var exposedMethod))
            {
                exposedMethod = LocateExposedMethod(context, serviceProvider, path, requestMessage);
            }

            if (exposedMethod != null)
            {
                if (_debugLogging)
                {
                    _logger?.LogDebug($"Found method for {path} {requestMessage.Method}");
                }

                return ExecuteMethod(context, serviceProvider, requestMessage, exposedMethod);
            }

            _logger?.LogError($"No method {requestMessage.Method} found at {path}");

            return ReturnMethodNotFound(requestMessage.Version, requestMessage.Id);
        }

        private IExposedMethodCache LocateExposedMethod(HttpContext context, IServiceProvider serviceProvider,
            string path, RequestMessage requestMessage)
        {
            var key = path + "|" + requestMessage.Method;

            if (_exposedMethodInformations.TryGetValue(key, out var methodInfo))
            {
                var cache = new ExposedMethodCache(methodInfo.Method, methodInfo.MethodName,
                    methodInfo.MethodAuthorizations,
                    methodInfo.Filters,
                    _namedParameterToArrayDelegateProvider,
                    _orderedParameterToArrayDelegateProvider,
                    _invokerBuilder);

                AddMethodCache(methodInfo.RouteNames, cache);

                return cache;
            }

            return null;
        }

        private void AddMethodCache(IEnumerable<string> names, IExposedMethodCache cache)
        {
            foreach (var name in names)
            {
                var methodKey = $"{name}|{cache.MethodName}";

                _methodCache.TryAdd(methodKey, cache);
            }
        }

        private async Task<ResponseMessage> ExecuteMethod(HttpContext context, IServiceProvider serviceProvider,
            RequestMessage requestMessage, IExposedMethodCache exposedMethod)
        {
            CallExecutionContext callExecutionContext =
                new CallExecutionContext(context, exposedMethod.InstanceType, exposedMethod.Method, requestMessage);

            if (exposedMethod.Authorizations.Length > 0)
            {
                for (var i = 0; i < exposedMethod.Authorizations.Length; i++)
                {
                    if (!await exposedMethod.Authorizations[i].AsyncAuthorize(callExecutionContext))
                    {
                        if (_debugLogging)
                        {
                            _logger?.LogDebug($"Unauthorized access to {context.Request.Path} {requestMessage.Method}");
                        }

                        return ReturnUnauthorizedAccess(context, requestMessage.Version, requestMessage.Id);
                    }
                }
            }

            object newInstance;

            try
            {
                newInstance = ActivatorUtilities.GetServiceOrCreateInstance(serviceProvider, exposedMethod.InstanceType);

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

                for (var i = 0; i < exposedFilters.Length; i++)
                {
                    filters.AddRange(exposedFilters[i](context));
                }
                
                runFilters = filters.Count > 0;
            }

            try
            {
                object[] parameterValues;

                if (requestMessage.Parameters == null)
                {
                    parameterValues = exposedMethod.OrderedParameterToArrayDelegate(NoParamsArray, context);
                }
                else if (requestMessage.Parameters is object[] objects)
                {
                    parameterValues = exposedMethod.OrderedParameterToArrayDelegate(objects, context);
                }
                else
                {
                    parameterValues = exposedMethod.NamedParametersToArrayDelegate(
                        (IDictionary<string, object>)requestMessage.Parameters, context);
                }

                if (runFilters)
                {
                    callExecutionContext.Parameters = parameterValues;

                    for (var i = 0; i < filters.Count; i++)
                    {
                        if (callExecutionContext.ContinueCall && 
                            filters[i] is ICallExecuteFilter executeFilter)
                        {
                            executeFilter.BeforeExecute(callExecutionContext);
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
                            executeFilter.AfterExecute(callExecutionContext);
                        }
                    }

                    return callExecutionContext.ResponseMessage;
                }

                return responseMessage;
            }
            catch (Exception exp)
            {
                return ExecuteMethodErrorHandler(context, requestMessage, exp, runFilters, filters, callExecutionContext);
            }
        }

        private ResponseMessage ExecuteMethodErrorHandler(HttpContext context, RequestMessage requestMessage, Exception exp,
            bool runFilters, List<ICallFilter> filters, CallExecutionContext callExecutionContext)
        {
            _logger?.LogError(EventIdCode.ExecutionException, exp,
                $"Exception thrown while processing {context.Request.Path} {requestMessage.Method} - " + exp.Message);

            if (runFilters)
            {
                foreach (var callFilter in filters)
                {
                    ICallExceptionFilter exceptionFilter = callFilter as ICallExceptionFilter;

                    exceptionFilter?.HandleException(callExecutionContext, exp);
                }
            }

            return ReturnInternalServerError(requestMessage.Version, requestMessage.Id,
                $"Executing {context.Request.Path} {requestMessage.Method} {exp.Message}");
        }

        private void WriteErrorMessage(HttpContext context, ErrorResponseMessage errorResponseMessage)
        {
            using (var responseStream = new StreamWriter(context.Response.Body))
            {
                using (var jsonStream = new JsonTextWriter(responseStream))
                {
                    _serializer.Serialize(jsonStream, errorResponseMessage);
                }
            }
        }

        private Task<ResponseMessage> ReturnMethodNotFound(string version, string id)
        {
            return
                Task.FromResult<ResponseMessage>(new ErrorResponseMessage(version, id, JsonRpcErrorCode.MethodNotFound,
                    "Method not found"));
        }

        private ResponseMessage ReturnInternalServerError(string version, string id, string expMessage)
        {
            var errorMessage = "Internal Server Error";

            if (_configuration.Value.ShowErrorMessage)
            {
                errorMessage += " " + expMessage;
            }

            return new ErrorResponseMessage(version, id, JsonRpcErrorCode.InternalServerError, errorMessage);
        }

        private ResponseMessage ReturnUnauthorizedAccess(HttpContext context, string version, string id)
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;

            return new ErrorResponseMessage(version, id, JsonRpcErrorCode.UnauthorizedAccess, "No access to this method");
        }
    }
}
