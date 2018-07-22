using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using EasyRpc.AspNetCore.Messages;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace EasyRpc.AspNetCore.Middleware
{
    public interface IExposedMethodCacheManager
    {
        void Configure(EndPointConfiguration configuration);

        IExposedMethodCache GetMethodCache(string path, string method);
    }

    public class ExposedMethodCacheManager : IExposedMethodCacheManager
    {
        private readonly ConcurrentDictionary<string, IExposedMethodCache> _methodCache =
            new ConcurrentDictionary<string, IExposedMethodCache>();
        private readonly IArrayMethodInvokerBuilder _invokerBuilder;
        private readonly IOptions<RpcServiceConfiguration> _configuration;

        private EndPointConfiguration _endPointConfiguration;

        public ExposedMethodCacheManager(IArrayMethodInvokerBuilder invokerBuilder, 
                                         IOptions<RpcServiceConfiguration> configuration)
        {
            _invokerBuilder = invokerBuilder;
            _configuration = configuration;
        }

        public void Configure(EndPointConfiguration configuration)
        {
            _endPointConfiguration = configuration;
        }

        public IExposedMethodCache GetMethodCache(string path, string method)
        {
            var methodKey = string.Concat(path, '*', method);

            if (!_methodCache.TryGetValue(methodKey, out var exposedMethod))
            {
                exposedMethod = LocateExposedMethod(methodKey);
            }

            return exposedMethod;
        }

        private IExposedMethodCache LocateExposedMethod(string key)
        {
            if (_endPointConfiguration.Methods.TryGetValue(key, out var methodInfo))
            {
                var cache = new ExposedMethodCache(methodInfo.MethodInfo, 
                    methodInfo.MethodName,
                    methodInfo.MethodAuthorizations,
                    methodInfo.Filters,
                    _invokerBuilder,
                    _configuration.Value.SupportResponseCompression);

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

    }
}
