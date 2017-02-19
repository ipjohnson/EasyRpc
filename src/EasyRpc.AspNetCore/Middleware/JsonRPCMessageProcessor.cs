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

        public JsonRpcMessageProcessor(ITypeManager typeManager, IJsonSerializerProvider provider, IOrderedParameterMethodInvokeBuilder orderedParameterMethodInvokeBuilder, INamedParameterMethodInvokerBuilder namedParameterMethodInvokerBuilder)
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
                foreach (var name in exposedMethod.Names)
                {
                    _exposedMethodInformations[name + "|" + exposedMethod.MethodName] = exposedMethod;
                }
            }
        }

        public Task ProcessRequest(HttpContext context)
        {
            try
            {
                RequestPackage requestPackage;

                using (var streamReader = new StreamReader(context.Request.Body))
                {
                    using (var jsonReader = new JsonTextReader(streamReader))
                    {
                        requestPackage = _serializer.Deserialize<RequestPackage>(jsonReader);
                    }
                }

                if (requestPackage.IsBulk)
                {
                    return ProcessBulkRequest(context, requestPackage);
                }

                return ProcessRequest(context, requestPackage.Requests.First());
            }
            catch (Exception)
            {

                throw;
            }
        }

        private Task ProcessBulkRequest(HttpContext context, RequestPackage requestPackage)
        {
            throw new NotImplementedException();
        }

        private async Task ProcessRequest(HttpContext context, RequestMessage requestMessage)
        {
            var path = context.Request.Path.Value.Substring(_route.Length).TrimEnd('/');

            var response = await ProcessIndividualRequest(context, context.RequestServices, path, requestMessage);

            using (var responseStream = new StreamWriter(context.Response.Body))
            {
                using (var jsonStream = new JsonTextWriter(responseStream))
                {
                    _serializer.Serialize(jsonStream, response);
                }
            }
        }

        private Task<ResponseMessage> ProcessIndividualRequest(HttpContext context, IServiceProvider serviceProvider, string path, RequestMessage requestMessage)
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

            return ReturnMethodNotFound();
        }

        private IExposedMethodCache LocateExposedMethod(HttpContext context, IServiceProvider serviceProvider, string path, RequestMessage requestMessage)
        {
            ExposedMethodInformation methodInfo;

            var key = path + "|" + requestMessage.Method;

            if (_exposedMethodInformations.TryGetValue(key, out methodInfo))
            {
                var cache = new ExposedMethodCache(methodInfo.Method, methodInfo.MethodName, _orderedParameterMethodInvokeBuilder, _namedParameterMethodInvokerBuilder);

                AddCache(methodInfo.Names, cache);

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

        private Task<ResponseMessage> ReturnMethodNotFound()
        {
            return Task.FromResult<ResponseMessage>(new ErrorResponseMessage { });
        }

        private Task<ResponseMessage> ExecuteMethod(HttpContext context, IServiceProvider serviceProvider, RequestMessage requestMessage, IExposedMethodCache exposedMethod)
        {
            var newInstance = ActivatorUtilities.GetServiceOrCreateInstance(serviceProvider, exposedMethod.InstanceType);

            if (requestMessage.Parameters == null ||
                requestMessage.Parameters is object[])
            {
                return exposedMethod.OrderedParametersExecution(requestMessage.Version, requestMessage.Id, newInstance,
                    (object[])requestMessage.Parameters);
            }

            return exposedMethod.NamedParametersExecution(requestMessage.Version, requestMessage.Id, newInstance,
                requestMessage.Parameters as IDictionary<string, object>);
        }
    }
}
