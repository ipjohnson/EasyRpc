using System;
using System.Reflection;
using EasyRpc.DynamicClient.Grace.Impl;
using EasyRpc.DynamicClient.ProxyGenerator;
using Grace.DependencyInjection;
using Newtonsoft.Json;

namespace EasyRpc.DynamicClient.Grace
{
    public interface IProxyConfiguration
    {
        /// <summary>
        /// Timeout in seconds
        /// </summary>
        /// <param name="timeoutValue"></param>
        /// <returns></returns>
        int Timeout(double timeoutValue = 30);
    }

    public interface IProxyCollectionConfiguration
    {
        IProxyConfiguration ProxyNamespace(string proxyNamespace, string url);

        IProxyConfiguration ProxyNamespaceContaining<T>(string url);
    }

    public class ProxyNamespaceConfig
    {
        public string Url { get; set; }

        public bool CallByName { get; set; }

        public bool UseDataContext { get; set; }

        public Action<JsonSerializer> SerializerInit { get; set; }

        public string[] Namespaces { get; set; }

        public Func<MethodInfo,bool> CompressRequest { get; set; }

        public Func<MethodInfo,bool> CompressResponse { get; set; }
    }

    public static class LanguageExtensions
    {
        public static void ProxyNamespace(this IExportRegistrationBlock block, ProxyNamespaceConfig config)
        {          
            // I mark everything as -1 priority so that if another version is registered it overrides these defaults
            block.Export<ProxyGenerator.ProxyGenerator>().As<IProxyGenerator>().WithPriority(-1).Lifestyle.Singleton().IfNotRegistered(typeof(IProxyGenerator));
            block.Export<DefaultNamingConventionService>().As<INamingConventionService>().WithPriority(-1).IfNotRegistered(typeof(INamingConventionService));
            block.ExportFactory(() => new DefaultRpcClientProvider(config.Url ?? "ReplaceMe")).As<IRpcHttpClientProvider>().WithPriority(-1).IfNotRegistered(typeof(IRpcHttpClientProvider));
            block.Export<JsonMethodObjectWriter>().As<IJsonMethodObjectWriter>().Lifestyle.Singleton().WithPriority(-1).IfNotRegistered(typeof(JsonMethodObjectWriter));
            block.Export<RpcProxyService>().As<IRpcProxyService>().WithPriority(-1).Lifestyle.SingletonPerScope().IfNotRegistered(typeof(IRpcProxyService));
            
            var serializerRegistration = 
                block.Export<JsonSerializer>().WithPriority(-1).Lifestyle.Singleton().IfNotRegistered(typeof(JsonSerializer));

            if (config.SerializerInit != null)
            {
                serializerRegistration.Apply(config.SerializerInit);
            }

            if (config.UseDataContext)
            {
                block.Export<DataContextHeaderProcessor>().As<IRpcContextHeader>().As<IHeaderProcessor>().WithPriority(-1).Lifestyle.SingletonPerScope().IfNotRegistered(typeof(IHeaderProcessor));
            }

            var compressionPicker = new DefaultMethodCompressionPicker(config.CompressRequest, config.CompressResponse);

            block.AddMissingExportStrategyProvider(new ProxyStrategyProvider(config.CallByName, compressionPicker, config.Namespaces ));
        }

        public static void ProxyNamespace(this IExportRegistrationBlock block, string url = null,
            bool callByName = true, bool useDataContext = false, params string[] namespaces)
        {
            block.ProxyNamespace(new ProxyNamespaceConfig
            {
                Url = url,
                Namespaces = namespaces,
                CallByName = callByName,
                UseDataContext = useDataContext,
                SerializerInit = null
            });
        }
        
        public static void ProxyNamespace(this IInjectionScope scope, string url = null, bool callByName = true, bool useDataContext = false, params string[] namespaces)
        {
            scope.Configure(c => c.ProxyNamespace(new ProxyNamespaceConfig
            {
                Url = url,
                Namespaces = namespaces,
                CallByName = callByName,
                UseDataContext = useDataContext,
                SerializerInit = null
            }));
        }
    }
}
