using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using EasyRpc.AspNetCore;
using EasyRpc.Tests.Services.SimpleServices;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Xunit;

namespace EasyRpc.Tests.AspNetCore
{
    public abstract class BaseRequestTest : IDisposable
    {
        protected ISharedStorage Shared = new SharedStorage();

        private HttpClient _client;
        private IHost _host;
        private JsonSerializerOptions _jsonSerializerOptions;

        protected async Task<HttpClient> Client()
        {
            return _client ??= await SetupClient();
        }

        protected async Task<HttpClient> SetupClient()
        {
            var hostBuilder = new HostBuilder()
                .ConfigureWebHost(webHost =>
                {
                    // Add TestServer
                    webHost.UseTestServer();
                    webHost.ConfigureServices(ConfigureServices);
                    webHost.Configure(app => app.UseRpcServices(ApiRegistration));
                });

            _host = await hostBuilder.StartAsync();

            return _host.GetTestClient();
        }

        protected virtual string BasePath => "/";

        protected virtual void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton(Shared);
            services.AddRpcServices();
        }

        protected abstract void ApiRegistration(IApiConfiguration api);

        public void Dispose()
        {
           _client?.Dispose();
            _host?.Dispose();
        }

        protected async Task<HttpResponseMessage> Post(string path, object postValue)
        {
            var client = await Client();

            var postContent = new StringContent(JsonSerializer.Serialize(postValue, postValue.GetType(), JsonSerializerOptions), Encoding.UTF8, "application/json");

            return await client.PostAsync(path, postContent);
        }

        protected async Task<HttpResponseMessage> Get(string path)
        {
            var client = await Client();

            return await client.GetAsync(path);
        }

        protected async Task<T> Deserialize<T>(HttpResponseMessage responseMessage, HttpStatusCode status = HttpStatusCode.OK)
        {
            if (responseMessage.StatusCode == status)
            {
                return JsonSerializer.Deserialize<T>(await responseMessage.Content.ReadAsStringAsync(), JsonSerializerOptions);
            }

            var message = await responseMessage.Content.ReadAsStringAsync();

            throw new Exception($"Expected Status code {status} received {responseMessage.StatusCode}{Environment.NewLine}{message}");
        }

        protected class InternalStartup
        {

        }

        public interface ISharedStorage
        {
            ConcurrentDictionary<string, object> Items { get; }
        }

        public class SharedStorage : ISharedStorage
        {
            public ConcurrentDictionary<string,object> Items { get; } = new ConcurrentDictionary<string, object>();
        }

        protected virtual JsonSerializerOptions JsonSerializerOptions => _jsonSerializerOptions ??=
            new JsonSerializerOptions {PropertyNamingPolicy = JsonNamingPolicy.CamelCase};
    }
}
