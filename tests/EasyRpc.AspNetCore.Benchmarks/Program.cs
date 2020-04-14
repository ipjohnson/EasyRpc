using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Running;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace EasyRpc.AspNetCore.Benchmarks
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var benchmark = new MiddleWareBenchmark();

            benchmark.Setup();
            benchmark.GetOneParam().Wait();
            //benchmark.ConcatBenchmark().Wait();

            //benchmark.CarterPlainText().Wait();
            benchmark.Shutdown();

            BenchmarkRunner.Run<MiddleWareBenchmark>(DefaultConfig
                .Instance
                .With(Job.InProcess.WithGcServer(true)));
            //CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
