using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using EasyRpc.AspNetCore.Serializers;
using EasyRpc.AspNetCore.Utf8Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;

namespace EasyRpc.AspNetCore.Benchmarks
{
    [MemoryDiagnoser]
    public class MiddleWareBenchmark
    {
        private HttpClient _easyClient;
        private WebApplicationFactory<EasyRpcStartup> _easyServices;

        private HttpClient _easyRoutingClient;
        private WebApplicationFactory<EasyRpcStartupAspNetRouting> _easyRoutingServices;

        private HttpClient _blankClient;
        private WebApplicationFactory<BlankStartup> _blankFactory;

        private HttpClient _carterClient;
        private WebApplicationFactory<CarterStartup> _carterFactory;

        [GlobalSetup]
        public void Setup()
        {
            _easyServices = new WebApplicationFactory<EasyRpcStartup>()
                .WithWebHostBuilder(configuration =>
                {
                    configuration.ConfigureLogging(logging => { logging.ClearProviders(); });
                    configuration.UseSolutionRelativeContentRoot("tests/EasyRpc.AspNetCore.Benchmarks/");
                    configuration.UseStartup<EasyRpcStartup>();
                });

            _easyClient = _easyServices.CreateClient();

            _easyRoutingServices = new WebApplicationFactory<EasyRpcStartupAspNetRouting>()
                .WithWebHostBuilder(configuration =>
                {
                    configuration.ConfigureLogging(logging => { logging.ClearProviders(); });
                    configuration.UseSolutionRelativeContentRoot("tests/EasyRpc.AspNetCore.Benchmarks/");
                    configuration.UseStartup<EasyRpcStartupAspNetRouting>();
                });

            _easyRoutingClient = _easyRoutingServices.CreateClient();

            _blankFactory = new WebApplicationFactory<BlankStartup>()
                .WithWebHostBuilder(configuration =>
                {
                    configuration.ConfigureLogging(logging => { logging.ClearProviders(); });
                    configuration.UseSolutionRelativeContentRoot("tests/EasyRpc.AspNetCore.Benchmarks/");
                    configuration.UseStartup<BlankStartup>();
                });

            _blankClient = _blankFactory.CreateClient();

            _carterFactory = new WebApplicationFactory<CarterStartup>().WithWebHostBuilder(configuration =>
            {
                configuration.ConfigureLogging(logging => { logging.ClearProviders(); });
                configuration.UseSolutionRelativeContentRoot("tests/EasyRpc.AspNetCore.Benchmarks/");
                configuration.UseStartup<CarterStartup>();
            });

            _carterClient = _carterFactory.CreateClient();
        }

        [GlobalCleanup]
        public void Shutdown()
        {

            _easyServices?.Dispose();
            _blankFactory?.Dispose();
            _carterFactory?.Dispose();
            _easyRoutingServices?.Dispose();

            _easyClient?.Dispose();
            _blankClient?.Dispose();
            _carterClient?.Dispose();
            _easyRoutingClient?.Dispose();
        }

        private readonly HttpContent _concatContent =
            new StringContent("{\"argA\": \"1\",\"argB\": \"Testing\", \"argC\": \"argC\", \"argD\": \"argD\"}", Encoding.UTF8, "application/json");

        [Benchmark]
        public async Task ConcatBenchmark()
        {
            var result = await _easyClient.PostAsync("/StringService/Concat", _concatContent);

            var stringResult = await result.Content.ReadAsStringAsync();
        }

        [Benchmark]
        public async Task CarterPlainText()
        {
            var result = await _carterClient.GetAsync("/plaintext");

            var stringResult = await result.Content.ReadAsStringAsync();
        }

        [Benchmark]
        public async Task PlainText()
        {
            var result = await _easyClient.GetAsync("/plaintext");

            var stringResult = await result.Content.ReadAsStringAsync();
        }

        [Benchmark]
        public async Task NoParams()
        {
            var result = await _easyClient.GetAsync("/noparams2");

            var stringResult = await result.Content.ReadAsStringAsync();
        }


        [Benchmark]
        public async Task GetOneParam()
        {
            var result = await _easyClient.GetAsync("/test/123");

            var stringResult = await result.Content.ReadAsStringAsync();
        }

        [Benchmark]
        public async Task PlainTextAspRouting()
        {
            var result = await _easyRoutingClient.GetAsync("/plaintext");

            var stringResult = await result.Content.ReadAsStringAsync();
        }

        [Benchmark]
        public async Task NoParamsAspRouting()
        {
            var result = await _easyRoutingClient.GetAsync("/noparams2");

            var stringResult = await result.Content.ReadAsStringAsync();
        }

        [Benchmark]
        public async Task BlankBenchmark()
        {
            var result = await _blankClient.GetAsync("/test");

            var stringResult = await result.Content.ReadAsStringAsync();
        }
    }

    public class EasyRpcStartup
    {
        public EasyRpcStartup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddRpcServices();
            services.AddTransient<Services.StringService>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseRpcRouting(api =>
            {
                api.Expose<Services.StringService>();
                api.Method.Get("/noparams2", () => new { Value = 1, Value2 = 2 });
                api.Method.Get("/test/{id}", (int id) => new { id });
            });
        }

        public class ReturnClass
        {
            public int Value { get; set; }
            public int Value2 { get; set; }
        }
    }

    public class EasyRpcStartupAspNetRouting
    {
        public EasyRpcStartupAspNetRouting(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddRpcServices(config => config.RegisterJsonSerializer = false);
            services.TryAddScoped<IContentSerializer, Utf8JsonContentSerializer>();
            services.AddTransient<Services.StringService>();
            
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseRouting();

            app.UseRpcRouting(api =>
            {
                api.Expose<Services.StringService>();
                api.Method.Get("/noparams2", () => new { Value = 1, Value2 = 2 });
            });
        }
    }
}
