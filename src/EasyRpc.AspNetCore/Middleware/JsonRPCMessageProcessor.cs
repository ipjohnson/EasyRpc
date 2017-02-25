using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using EasyRpc.AspNetCore.Messages;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace EasyRpc.AspNetCore.Middleware
{
    public class RpcMethodCache : ConcurrentDictionary<string, IExposedMethodCache>
    {
    }

    public interface IJsonRpcMessageProcessor
    {
        void Configure(IApiConfigurationProvider configuration, string route);

        Task ProcessRequest(HttpContext context);
    }

    public class JsonRpcMessageProcessor : IJsonRpcMessageProcessor
    {
        private readonly ConcurrentDictionary<string, RpcMethodCache> _methodCache;
        private readonly JsonSerializer _serializer;
        private readonly ConcurrentDictionary<string, ExposedMethodInformation> _exposedMethodInformations;
        private readonly IOrderedParameterMethodInvokeBuilder _orderedParameterMethodInvokeBuilder;
        private readonly INamedParameterMethodInvokerBuilder _namedParameterMethodInvokerBuilder;
        private readonly IOptions<RpcServiceConfiguration> _configuration;
        private readonly ILogger<JsonRpcMessageProcessor> _logger;
        private string _route;

        public JsonRpcMessageProcessor(IJsonSerializerProvider provider,
            IOrderedParameterMethodInvokeBuilder orderedParameterMethodInvokeBuilder,
            INamedParameterMethodInvokerBuilder namedParameterMethodInvokerBuilder,
            IOptions<RpcServiceConfiguration> configuration,
            ILogger<JsonRpcMessageProcessor> logger = null)
        {
            _orderedParameterMethodInvokeBuilder = orderedParameterMethodInvokeBuilder;
            _namedParameterMethodInvokerBuilder = namedParameterMethodInvokerBuilder;
            _configuration = configuration;
            _logger = logger;
            _serializer = provider.ProvideSerializer();
            _methodCache = new ConcurrentDictionary<string, RpcMethodCache>();
            _exposedMethodInformations = new ConcurrentDictionary<string, ExposedMethodInformation>();
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

            if (_configuration.Value.DebugLogging)
            {
                _logger?.LogInformation(new EventId(10), $"Processing json-rpc request for path {context.Request.Path}");
            }

            try
            {
                using (var streamReader = new StreamReader(context.Request.Body))
                {
                    using (var jsonReader = new JsonTextReader(streamReader))
                    {
                        requestPackage = _serializer.Deserialize<RequestPackage>(jsonReader);
                    }
                }
            }
            catch (Exception exp)
            {
                _logger?.LogError("Exception thrown while deserializing request package: " + exp.Message);

                WriteErrorMessage(context,
                       new ErrorResponseMessage("2.0", "", JsonRpcErrorCode.InvalidRequest, "Could not parse request"));

                return Task.CompletedTask;
            }

            if (requestPackage == null)
            {
                WriteErrorMessage(context,
                     new ErrorResponseMessage("2.0", "", JsonRpcErrorCode.InvalidRequest, "Could not parse request"));

                return Task.CompletedTask;
            }

            if (requestPackage.IsBulk)
            {
                return ProcessBulkRequest(context, requestPackage);
            }

            return ProcessRequest(context, requestPackage.Requests.First());

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
                using (var responseStream = new StreamWriter(context.Response.Body))
                {
                    using (var jsonStream = new JsonTextWriter(responseStream))
                    {
                        _serializer.Serialize(jsonStream, values);
                    }
                }
            }
            catch (Exception exp)
            {
                _logger?.LogError("Exception thrown while serializing bulk output: " + exp.Message);

                WriteErrorMessage(context,
                    new ErrorResponseMessage("2.0", "",
                        JsonRpcErrorCode.InternalServerError, "Internal Server Error"));
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
                using (var responseStream = new StreamWriter(context.Response.Body))
                {
                    using (var jsonStream = new JsonTextWriter(responseStream))
                    {
                        _serializer.Serialize(jsonStream, response);
                    }
                }
            }
            catch (Exception exp)
            {
                _logger?.LogError("Exception thrown while serializing response: " + exp.Message);

                WriteErrorMessage(context,
                    new ErrorResponseMessage(requestMessage.Version, requestMessage.Id,
                        JsonRpcErrorCode.InternalServerError, "Internal Server Error"));
            }
        }

        private Task<ResponseMessage> ProcessIndividualRequest(HttpContext context, IServiceProvider serviceProvider,
            string path, RequestMessage requestMessage)
        {
            RpcMethodCache cache;
            IExposedMethodCache exposedMethod = null;

            if (_methodCache.TryGetValue(path, out cache))
            {
                cache.TryGetValue(requestMessage.Method, out exposedMethod);
            }

            if (exposedMethod == null)
            {
                exposedMethod = LocateExposedMethod(context, serviceProvider, path, requestMessage);
            }

            if (exposedMethod != null)
            {
                if (_configuration.Value.DebugLogging)
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
            ExposedMethodInformation methodInfo;

            var key = path + "|" + requestMessage.Method;

            if (_exposedMethodInformations.TryGetValue(key, out methodInfo))
            {
                var cache = new ExposedMethodCache(methodInfo.Method, methodInfo.MethodName,
                    _orderedParameterMethodInvokeBuilder, _namedParameterMethodInvokerBuilder,
                    methodInfo.MethodAuthorizations,
                    methodInfo.Filters);

                AddCache(methodInfo.RouteNames, cache);

                return cache;
            }

            return null;
        }

        private void AddCache(IEnumerable<string> names, IExposedMethodCache cache)
        {
            foreach (var name in names)
            {
                _methodCache.AddOrUpdate(name,
                    s =>
                    {
                        var newDictionary = new RpcMethodCache();

                        newDictionary[cache.MethodName] = cache;

                        return newDictionary;
                    },
                    (s, methodCache) =>
                    {
                        methodCache[cache.MethodName] = cache;

                        return methodCache;
                    }
                );
            }
        }

        private async Task<ResponseMessage> ExecuteMethod(HttpContext context, IServiceProvider serviceProvider,
            RequestMessage requestMessage, IExposedMethodCache exposedMethod)
        {
            if (exposedMethod.Authorizations.Length > 0)
            {
                foreach (var authorization in exposedMethod.Authorizations)
                {
                    if (!await authorization.AsyncAuthorize(context))
                    {
                        if (_configuration.Value.DebugLogging)
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
            }
            catch (Exception exp)
            {
                _logger?.LogError($"Exception thrown while creating instance {exposedMethod.InstanceType.Name} for {context.Request.Path} {requestMessage.Method} - " + exp.Message);

                // log error 
                return ReturnInternalServerError(requestMessage.Version, requestMessage.Id);
            }

            if (newInstance == null)
            {
                return ReturnInternalServerError(requestMessage.Version, requestMessage.Id);
            }

            List<ICallFilter> filters = null;
            ICallExecutionContext callExecutionContext = null;

            if (exposedMethod.Filters.Length > 0)
            {
                filters = new List<ICallFilter>();

                foreach (var func in exposedMethod.Filters)
                {
                    filters.AddRange(func(context));
                }

                if (filters.Count > 0)
                {
                    callExecutionContext = new CallExecutionContext(context, exposedMethod.InstanceType, requestMessage);
                }
            }

            try
            {
                ResponseMessage responseMessage;

                if (callExecutionContext != null)
                {
                    foreach (var callFilter in filters)
                    {
                        ICallExecuteFilter executeFilter = callFilter as ICallExecuteFilter;

                        if (executeFilter != null &&
                            callExecutionContext.ContinueCall)
                        {
                            executeFilter.BeforeExecute(callExecutionContext);
                        }
                    }
                }

                if (callExecutionContext == null || callExecutionContext.ContinueCall)
                {
                    if (requestMessage.Parameters == null ||
                        requestMessage.Parameters is object[])
                    {
                        responseMessage =
                            await exposedMethod.OrderedParametersExecution(requestMessage.Version,
                                requestMessage.Id,
                                newInstance,
                                (object[])requestMessage.Parameters,
                                context);
                    }
                    else
                    {
                        responseMessage =
                            await exposedMethod.NamedParametersExecution(requestMessage.Version,
                                requestMessage.Id,
                                newInstance,
                                (IDictionary<string, object>)requestMessage.Parameters,
                                context);
                    }
                }
                else
                {
                    return callExecutionContext.ResponseMessage;
                }

                if (callExecutionContext != null)
                {
                    callExecutionContext.ResponseMessage = responseMessage;

                    foreach (var callFilter in filters)
                    {
                        ICallExecuteFilter executeFilter = callFilter as ICallExecuteFilter;

                        if (executeFilter != null &&
                            callExecutionContext.ContinueCall)
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
                _logger?.LogError($"Exception thrown while processing {context.Request.Path} {requestMessage.Method} - " + exp.Message);

                if (callExecutionContext != null)
                {
                    foreach (var callFilter in filters)
                    {
                        ICallExceptionFilter exceptionFilter = callFilter as ICallExceptionFilter;

                        exceptionFilter?.HandleException(callExecutionContext, exp);
                    }
                }

                return ReturnInternalServerError(requestMessage.Version, requestMessage.Id);
            }
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

        private ResponseMessage ReturnInternalServerError(string version, string id)
        {
            return new ErrorResponseMessage(version, id, JsonRpcErrorCode.InternalServerError, "Internal Server");
        }

        private ResponseMessage ReturnUnauthorizedAccess(HttpContext context, string version, string id)
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;

            return new ErrorResponseMessage(version, id, JsonRpcErrorCode.UnauthorizedAccess, "No access to this method");
        }
    }
}
