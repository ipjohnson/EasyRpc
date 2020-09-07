using System;
using System.Reflection;
using System.Threading.Tasks;
using EasyRpc.DynamicClient;
using EasyRpc.DynamicClient.CodeGeneration;
using EasyRpc.DynamicClient.Grace;
using EasyRpc.DynamicClient.MessagePack;
using EasyRpc.Examples.SharedDefinitions;
using Grace.DependencyInjection;

namespace EasyRpc.Examples.ConsoleClient
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var container = new DependencyInjectionContainer();

            container.Configure(c =>
            {
                c.ExportAs<MessagePackClientSerializationTypeAttributor, IClientSerializationTypeAttributor>();
                c.ProxyNamespace("https://localhost:5001/", serializer: new MessagePackClientSerializer(), namingConvention: new InterfaceNamingConvention(),
                        namespaces: "EasyRpc.Examples.SharedDefinitions");
            });

            var mathService = container.Locate<IMathService>();

            var result = await mathService.Add(5, 10);

            foreach (var provider in container.MissingExportStrategyProviders)
            {
                if (provider is IDisposable disposable)
                {
                    disposable.Dispose();
                }
            }
            container.Dispose();
        }
    }


    public class InterfaceNamingConvention : INamingConventionService
    {
        /// <inheritdoc />
        public string GetNameForType(Type type)
        {
            if (type.IsInterface && type.Name.StartsWith('I'))
            {
                return type.Name.Substring(1);
            }

            return type.Name;
        }

        /// <inheritdoc />
        public string GetMethodName(MethodInfo method)
        {
            return method.Name;
        }
    }
}
