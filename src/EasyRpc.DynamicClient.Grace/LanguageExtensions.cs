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
        /// <summary>
        /// Proxy a namespace
        /// </summary>
        /// <param name="block"></param>
        /// <param name="url"></param>
        /// <param name="useDataContext"></param>
        /// <param name="serializer"></param>
        /// <param name="namingConvention"></param>
        /// <param name="namespaces"></param>
        public static void ProxyNamespace(this IExportRegistrationBlock block, 
            string url = null,
            bool useDataContext = false, 
            IClientSerializer serializer = null,
            INamingConventionService namingConvention = null,
            params string[] namespaces)
        {
            RegisterExports(block);

            SetupProxyInfo(block, url, useDataContext, serializer, namingConvention, namespaces);
        }

        /// <summary>
        /// Proxy a namespace with a specific config
        /// </summary>
        /// <param name="block"></param>
        /// <param name="config"></param>
        public static void ProxyNamespace(this IExportRegistrationBlock block, ProxyNamespaceConfig config)
        {
            RegisterExports(block);

            block.AddMissingExportStrategyProvider(new ProxyStrategyProvider(config));
        }

        private static void SetupProxyInfo(IExportRegistrationBlock block, 
            string url, 
            bool useDataContext, 
            IClientSerializer serializer,
            INamingConventionService namingConvention, 
            string[] namespaces)
        {
            var config = new ProxyNamespaceConfig
            {
                Url = url,
                UseDataContext = useDataContext,
                Namespaces = namespaces,
                Serializer = serializer,
                NamingConvention = namingConvention
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
