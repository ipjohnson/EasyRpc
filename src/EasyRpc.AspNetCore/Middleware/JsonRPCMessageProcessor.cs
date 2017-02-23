using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using EasyRpc.AspNetCore.Messages;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
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
        private readonly ITypeManager _typeManager;
        private readonly ConcurrentDictionary<string, RpcMethodCache> _methodCache;
        private readonly JsonSerializer _serializer;
        private readonly ConcurrentDictionary<string, ExposedMethodInformation> _exposedMethodInformations;
        private readonly IOrderedParameterMethodInvokeBuilder _orderedParameterMethodInvokeBuilder;
        private readonly INamedParameterMethodInvokerBuilder _namedParameterMethodInvokerBuilder;
        private string _route;

        public JsonRpcMessageProcessor(ITypeManager typeManager, IJsonSerializerProvider provider,
            IOrderedParameterMethodInvokeBuilder orderedParameterMethodInvokeBuilder,
            INamedParameterMethodInvokerBuilder namedParameterMethodInvokerBuilder)
        {
            _typeManager = typeManager;
            _orderedParameterMethodInvokeBuilder = orderedParameterMethodInvokeBuilder;
            _namedParameterMethodInvokerBuilder = namedParameterMethodInvokerBuilder;
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

            try
            {
                // set as ok by default
                context.Response.StatusCode = StatusCodes.Status200OK;

                using (var streamReader = new StreamReader(context.Request.Body))
                {
                    using (var jsonReader = new JsonTextReader(streamReader))
                    {
                        requestPackage = _serializer.Deserialize<RequestPackage>(jsonReader);
                    }
                }
            }
            catch (Exception)
            {
                // log error 
            }

            if (requestPackage == null)
            {
                return WriteErrorMessage(context,
                    new ErrorResponseMessage("2.0", "", JsonRpcErrorCode.InvalidRequest, "Could not parse request"));
            }

            if (requestPackage.IsBulk)
            {
                return ProcessBulkRequest(context, requestPackage);
            }

            return ProcessRequest(context, requestPackage.Requests.First());

        }

        private Task ProcessBulkRequest(HttpContext context, RequestPackage requestPackage)
        {
            throw new NotImplementedException();
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
            catch (Exception)
            {
                await WriteErrorMessage(context,
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
                return ExecuteMethod(context, serviceProvider, requestMessage, exposedMethod);
            }

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
                        return ReturnUnauthorizedAccess(context, requestMessage.Version, requestMessage.Id);
                    }
                }
            }

            object newInstance;

            try
            {
                newInstance = ActivatorUtilities.GetServiceOrCreateInstance(serviceProvider, exposedMethod.InstanceType);
            }
            catch (Exception)
            {
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

        private Task WriteErrorMessage(HttpContext context, ErrorResponseMessage errorResponseMessage)
        {
            using (var responseStream = new StreamWriter(context.Response.Body))
            {
                using (var jsonStream = new JsonTextWriter(responseStream))
                {
                    _serializer.Serialize(jsonStream, errorResponseMessage);
                }
            }

            return Task.CompletedTask;
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
