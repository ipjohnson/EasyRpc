using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using EasyRpc.Abstractions.Path;
using EasyRpc.DynamicClient.CodeGeneration;
using EasyRpc.DynamicClient.ExecutionService;
using EasyRpc.DynamicClient.Serializers;
using Grace.DependencyInjection;
using Grace.DependencyInjection.Impl.CompiledStrategies;

namespace EasyRpc.DynamicClient.Grace.Impl
{
    public class ProxyStrategyProvider : IMissingExportStrategyProvider, IDisposable
    {
        private readonly ProxyNamespaceConfig _namespaceConfig;
        private readonly IRpcHttpClientProvider _clientProvider;
        private IClientSerializer _clientSerializer;
        private readonly HttpClient _client;
        
        public ProxyStrategyProvider(ProxyNamespaceConfig namespaceConfig)
        {
            _namespaceConfig = namespaceConfig;

            _client = namespaceConfig.CreateClient?.Invoke("") ?? new HttpClient();

            _client.BaseAddress = new Uri(_namespaceConfig.Url);

            _clientProvider = new RpcHttpClientProvider(_client);
        }
        
        /// <inheritdoc />
        public bool CanLocate(IInjectionScope scope, IActivationExpressionRequest request)
        {
            var fullName = request.ActivationType.FullName;

            return (request.ActivationType.GetTypeInfo().IsInterface ||
                    request.ActivationType.GetTypeInfo().IsAbstract) &&
                   fullName != null &&
                   _namespaceConfig.Namespaces.Any(proxyNamespace => fullName.StartsWith(proxyNamespace));
        }

        /// <inheritdoc />
        public IEnumerable<IActivationStrategy> ProvideExports(IInjectionScope scope, IActivationExpressionRequest request)
        {
            var fullName = request.ActivationType.FullName;

            if ((request.ActivationType.GetTypeInfo().IsInterface ||
                 request.ActivationType.GetTypeInfo().IsAbstract) &&
                _namespaceConfig.Namespaces.Any(proxyNamespace => fullName.StartsWith(proxyNamespace)))
            {
                if (_namespaceConfig.Serializer == null)
                {
                    _namespaceConfig.Serializer = scope.LocateOrDefault<IClientSerializer>();

#if NETCOREAPP3_1
                    _namespaceConfig.Serializer = new JsonClientSerializer();
#endif

                    if (_namespaceConfig.Serializer == null)
                    {
                        throw new Exception("IClientSerializer implementation is not exported");
                    }
                }

                var implementationGenerator = scope.Locate<IServiceImplementationGenerator>();

                var implementationRequest = new ImplementationRequest
                {
                    DefaultSerializer = _namespaceConfig.Serializer,
                    ExposeDefaultMethod = ExposeDefaultMethod.PostOnly,
                    ClientProvider = _clientProvider,
                    InterfaceType = request.ActivationType,
                    NamingConventionService = _namespaceConfig.NamingConvention
                };

                var implementationType =
                    implementationGenerator.GenerateImplementationForInterface(implementationRequest);

                var strategy = new CompiledExportStrategy(implementationType, scope,
                    request.Services.LifestyleExpressionBuilder);

                strategy.AddExportAs(request.ActivationType);

                return new IActivationStrategy[]{ strategy };
            }

            return Array.Empty<IActivationStrategy>();
        }

        /// <inheritdoc />
        public void Dispose()
        {
            _client.Dispose();
        }
    }
}
