using System;
using EasyRpc.DynamicClient.CodeGeneration;
using EasyRpc.DynamicClient.ExecutionService;
using EasyRpc.DynamicClient.Grace.Impl;
using EasyRpc.DynamicClient.Serializers;
using Grace.DependencyInjection;

namespace EasyRpc.DynamicClient.Grace
{

    public static class LanguageExtensions
    {
        public static void ProxyNamespace(this IExportRegistrationBlock block, 
            string url = null,
            bool useDataContext = false, 
            IClientSerializer serializer = null,
            params string[] namespaces)
        {
            RegisterExports(block);

            SetupProxyInfo(block, url, useDataContext, serializer, namespaces);
        }

        private static void SetupProxyInfo(IExportRegistrationBlock block, string url, bool useDataContext, IClientSerializer serializer, string[] namespaces)
        {
            var config = new ProxyNamespaceConfig
            {
                Url = url,
                UseDataContext = useDataContext,
                Namespaces = namespaces,
                Serializer = serializer
            };

            block.AddMissingExportStrategyProvider(new ProxyStrategyProvider(config));
        }

        private static void RegisterExports(IExportRegistrationBlock block)
        {
            block.ExportAs<SerializationTypeCreator, ISerializationTypeCreator>().Lifestyle.Singleton()
                .IfNotRegistered(typeof(ISerializationTypeCreator));

            block.ExportAs<ServiceImplementationGenerator, IServiceImplementationGenerator>().Lifestyle.Singleton()
                .IfNotRegistered(typeof(IServiceImplementationGenerator));

            block.ExportAs<RpcExecutionService, IRpcExecutionService>().Lifestyle.Singleton()
                .IfNotRegistered(typeof(IRpcExecutionService));

        }
    }
}
