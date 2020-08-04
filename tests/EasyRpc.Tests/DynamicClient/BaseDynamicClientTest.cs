using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using EasyRpc.Abstractions.Path;
using EasyRpc.DynamicClient.CodeGeneration;
using EasyRpc.DynamicClient.ExecutionService;
using EasyRpc.DynamicClient.Serializers;
using Grace.DependencyInjection;
using Grace.DependencyInjection.Impl.CompiledStrategies;

namespace EasyRpc.Tests.DynamicClient
{
    public abstract class BaseDynamicClientTest
    {
        protected DependencyInjectionContainer Container { get; } = new DependencyInjectionContainer();

        protected abstract Task<HttpResponseMessage> ProcessRequest(HttpRequestMessage requestMessage);


        protected BaseDynamicClientTest()
        {
            Container.Configure(c =>
            {
                c.ExportAs<SerializationTypeCreator, ISerializationTypeCreator>().Lifestyle.Singleton()
                    .IfNotRegistered(typeof(ISerializationTypeCreator));

                c.ExportAs<ServiceImplementationGenerator, IServiceImplementationGenerator>().Lifestyle.Singleton()
                    .IfNotRegistered(typeof(IServiceImplementationGenerator));

                c.ExportAs<RpcExecutionService, IRpcExecutionService>().Lifestyle.Singleton()
                    .IfNotRegistered(typeof(IRpcExecutionService));
            });
        }

        protected T GetService<T>()
        {
            return Container.LocateOrDefault<T>() ?? CreateAndExportService<T>();
        }

        protected virtual IClientSerializer Serializer => new JsonClientSerializer();

        protected virtual async Task<HttpResponseMessage> SerializeResponse<T>(T value, HttpStatusCode statusCode = HttpStatusCode.OK)
        {
            var bytes = await Serializer.Serialize(value);

            return new HttpResponseMessage(statusCode) { Content = new ByteArrayContent(bytes) };
        }

        protected virtual async Task<T> DeserializeRequest<T>(HttpRequestMessage request)
        {
            var bytes = await request.Content.ReadAsByteArrayAsync();

            return await Serializer.Deserialize<T>(bytes);
        }


        private T CreateAndExportService<T>()
        {
            var implementationGenerator = Container.Locate<IServiceImplementationGenerator>();

            var implementationRequest = new ImplementationRequest
            {
                DefaultSerializer = Serializer,
                ExposeDefaultMethod = ExposeDefaultMethod.PostOnly,
                ClientProvider = new RpcTestClientProvider(ProcessRequest),
                InterfaceType = typeof(T)
            };

            var implementationType =
                implementationGenerator.GenerateImplementationForInterface(implementationRequest);

            Container.Configure(c =>
            {
                var strategy = new CompiledExportStrategy(implementationType, Container, c.OwningScope.StrategyCompiler.DefaultStrategyExpressionBuilder);

                strategy.AddExportAs(typeof(T));

                c.AddActivationStrategy(strategy);
            });

            return Container.Locate<T>();
        }
    }
}
