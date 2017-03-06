using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EasyRpc.DynamicClient.Grace.Impl;
using EasyRpc.DynamicClient.ProxyGenerator;
using Grace.DependencyInjection;

namespace EasyRpc.DynamicClient.Grace
{
    public static class LanguageExtensions
    {
        public static void ProxyNamespace(this IInjectionScope scope, string url, bool callByName, params string[] parameters)
        {
            scope.Configure(c =>
            {
                // I mark everything as -1 priority so that if another version is registered it overrides these defaults
                c.Export<ProxyGenerator.ProxyGenerator>().As<IProxyGenerator>().WithPriority(-1).Lifestyle.Singleton();
                c.Export<DefaultNamingConventionService>().As<INamingConventionService>().WithPriority(-1);
                c.ExportInstance(() => new DefaultRpcClientProvider(url)).As<IRpcHttpClientProvider>().WithPriority(-1);
                c.Export<DataContextHeaderProcessor>().As<IRpcContextHeader>().As<IHeaderProcessor>().WithPriority(-1).Lifestyle.SingletonPerScope();
                c.AddMissingExportStrategyProvider(new ProxyStrategyProvider(callByName,parameters));
            });
        }
    }
}
