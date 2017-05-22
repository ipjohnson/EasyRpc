using System;
using System.Collections.Generic;
using System.Linq;
using Grace.DependencyInjection;
using Grace.DependencyInjection.Impl.CompiledStrategies;
using System.Reflection;
using EasyRpc.DynamicClient.ProxyGenerator;

namespace EasyRpc.DynamicClient.Grace.Impl
{
    public class ProxyStrategyProvider : IMissingExportStrategyProvider
    {
        private readonly bool _callByName;
        private readonly string[] _proxyNamespaces;

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="callByName"></param>
        /// <param name="proxyNamespaces">provide namespaces that will be proxied</param>
        public ProxyStrategyProvider(bool callByName, params string[] proxyNamespaces)
        {
            _callByName = callByName;
            _proxyNamespaces = proxyNamespaces ?? throw new ArgumentNullException(nameof(proxyNamespaces));
        }

        /// <summary>Provide exports for a missing type</summary>
        /// <param name="scope">scope to provide value</param>
        /// <param name="request">request</param>
        /// <returns>set of activation strategies</returns>
        public IEnumerable<IActivationStrategy> ProvideExports(IInjectionScope scope, IActivationExpressionRequest request)
        {
            var fullName = request.ActivationType.FullName;

            if ((request.ActivationType.GetTypeInfo().IsInterface ||
                 request.ActivationType.GetTypeInfo().IsAbstract) &&
                _proxyNamespaces.Any(proxyNamespace => fullName.StartsWith(proxyNamespace)))
            {
                var proxyGenerator = scope.Locate<IProxyGenerator>();

                var proxyType = proxyGenerator.GenerateProxyType(request.ActivationType, _callByName);

                var strategy =
                    new CompiledExportStrategy(proxyType, scope, request.Services.LifestyleExpressionBuilder);
                
                strategy.AddExportAs(request.ActivationType);

                yield return strategy;
            }
        }
    }
}
