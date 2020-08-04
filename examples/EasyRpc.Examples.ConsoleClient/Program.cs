using System;
using System.Threading.Tasks;
using EasyRpc.DynamicClient.Grace;
using EasyRpc.Examples.SharedDefinitions;
using Grace.DependencyInjection;

namespace EasyRpc.Examples.ConsoleClient
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var container = new DependencyInjectionContainer();
            
            container.Configure(c => c.ProxyNamespace("https://localhost:44317/", namespaces: "EasyRpc.Examples.SharedDefinitions"));

            var mathService = container.Locate<IMathService>();

            var result = await mathService.Add(5, 10);
        }
    }
}
