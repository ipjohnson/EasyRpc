using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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

    public static class LanguageExtensions
    {
        public static void SetupProxys(this IInjectionScope scope, Action<IProxyCollectionConfiguration> configuration)
        {
            scope.Configure(c =>
            {
                if (!c.OwningScope.CanLocate(typeof(IProxyGenerator)))
                {
                    
                }
            });
        }


        public static void ProxyNamespace(this IInjectionScope scope, string url, bool callByName, params string[] parameters)
        {
            scope.Configure(c =>
            {
                // I mark everything as -1 priority so that if another version is registered it overrides these defaults
                c.Export<ProxyGenerator.ProxyGenerator>().As<IProxyGenerator>().WithPriority(-1).Lifestyle.Singleton();
                c.Export<DefaultNamingConventionService>().As<INamingConventionService>().WithPriority(-1);
                c.ExportInstance(() => new DefaultRpcClientProvider(url)).As<IRpcHttpClientProvider>().WithPriority(-1);
                c.Export<DataContextHeaderProcessor>().As<IRpcContextHeader>().As<IHeaderProcessor>().WithPriority(-1).Lifestyle.SingletonPerScope();
                c.Export<JsonMethodObjectWriter>().As<IJsonMethodObjectWriter>().Lifestyle.Singleton().WithPriority(-1);
                c.Export<RpcProxyService>().As<IRpcProxyService>().WithPriority(-1).Lifestyle.SingletonPerScope();
                c.Export<JsonSerializer>().Lifestyle.Singleton();

                c.AddMissingExportStrategyProvider(new ProxyStrategyProvider(callByName,parameters));
            });
        }
    }
}
