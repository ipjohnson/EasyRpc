using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using EasyRpc.AspNetCore;
using EasyRpc.AspNetCore.Serializers;
using EasyRpc.DynamicClient.Serializers;
using EasyRpc.Tests.Services.SimpleServices;
using Microsoft.AspNetCore.Builder;
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
                    webHost.Configure(ConfigureAspNetPipeline);
                });

            _host = await hostBuilder.StartAsync();

            return _host.GetTestClient();
        }

        protected virtual void ConfigureAspNetPipeline(IApplicationBuilder app)
        {
            app.UseRpcRouting(ApiRegistration);
        }

        protected virtual string BasePath => "/";

        protected virtual string AcceptEncoding { get; set; }

        protected virtual void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton(Shared);
            services.AddRpcServices();
        }

        protected abstract void ApiRegistration(IRpcApi api);

        public void Dispose()
        {
            _client?.Dispose();
            _host?.Dispose();
        }

        protected Task<HttpResponseMessage> SendAsync(string httpMethod, string path, object postValue = null)
        {
            if (string.Equals(HttpMethod.Post.Method, httpMethod, StringComparison.CurrentCultureIgnoreCase))
            {
                return SendAsync(HttpMethod.Post, path, postValue);
            }

            if (string.Equals(HttpMethod.Put.Method, httpMethod, StringComparison.CurrentCultureIgnoreCase))
            {
                return SendAsync(HttpMethod.Put, path, postValue);
            }
            
            if (string.Equals(HttpMethod.Patch.Method, httpMethod, StringComparison.CurrentCultureIgnoreCase))
            {
                return SendAsync(HttpMethod.Patch, path, postValue);
            }
            
            if (string.Equals(HttpMethod.Get.Method, httpMethod, StringComparison.CurrentCultureIgnoreCase))
            {
                return SendAsync(HttpMethod.Get, path, postValue);
            }

            if (string.Equals(HttpMethod.Head.Method, httpMethod, StringComparison.CurrentCultureIgnoreCase))
            {
                return SendAsync(HttpMethod.Head, path, postValue);
            }

            if (string.Equals(HttpMethod.Options.Method, httpMethod, StringComparison.CurrentCultureIgnoreCase))
            {
                return SendAsync(HttpMethod.Options, path, postValue);
            }

            throw new Exception("Unknown method type " + httpMethod);
        }



        protected async Task<HttpResponseMessage> SendAsync(HttpMethod httpMethod, string path, object postValue = null)
        {
            var client = await Client();

            var request = new HttpRequestMessage(httpMethod, path);

            if (postValue != null)
            {
                HttpContent postContent;

                if (CustomSerializer != null)
                {
                    var bytes = await CustomSerializer.Serialize(postValue);

                    postContent = new ByteArrayContent(bytes);
                    postContent.Headers.ContentType = new MediaTypeHeaderValue(CustomSerializer.ContentType);
                }
                else
                {
                    postContent =
                        new StringContent(
                            JsonSerializer.Serialize(postValue, postValue.GetType(), JsonSerializerOptions),
                            Encoding.UTF8, "application/json");
                }

                request.Content = postContent;
            }

            client.DefaultRequestHeaders.AcceptEncoding.Clear();

            if (!string.IsNullOrEmpty(AcceptEncoding))
            {
                client.DefaultRequestHeaders.AcceptEncoding.Clear();
                client.DefaultRequestHeaders.AcceptEncoding.ParseAdd(AcceptEncoding);
            }

            return await client.SendAsync(request);
        }

        protected async Task<HttpResponseMessage> Post(string path, object postValue)
        {
            var client = await Client();
            HttpContent postContent;

            if (CustomSerializer != null)
            {
                var bytes = await CustomSerializer.Serialize(postValue);

                postContent = new ByteArrayContent(bytes);
                postContent.Headers.ContentType = new MediaTypeHeaderValue(CustomSerializer.ContentType);
            }
            else
            {
                postContent = new StringContent(JsonSerializer.Serialize(postValue, postValue.GetType(), JsonSerializerOptions), Encoding.UTF8, "application/json");
            }

            client.DefaultRequestHeaders.AcceptEncoding.Clear();

            if (!string.IsNullOrEmpty(AcceptEncoding))
            {
                client.DefaultRequestHeaders.AcceptEncoding.Clear();
                client.DefaultRequestHeaders.AcceptEncoding.ParseAdd(AcceptEncoding);
            }


            return await client.PostAsync(path, postContent);
        }

        protected async Task<HttpResponseMessage> Get(string path)
        {
            var client = await Client();

            client.DefaultRequestHeaders.AcceptEncoding.Clear();

            if (!string.IsNullOrEmpty(AcceptEncoding))
            {
                client.DefaultRequestHeaders.AcceptEncoding.Clear();
                client.DefaultRequestHeaders.AcceptEncoding.ParseAdd(AcceptEncoding);
            }

            return await client.GetAsync(path);
        }

        protected async Task<T> Deserialize<T>(HttpResponseMessage responseMessage,
            HttpStatusCode status = HttpStatusCode.OK)
        {
            if (CustomSerializer != null)
            {
                if (responseMessage.StatusCode == status)
                {
                    return await CustomSerializer.Deserialize<T>(await responseMessage.Content.ReadAsByteArrayAsync());
                }

                throw new Exception(
                    $"Expected Status code {status} received {responseMessage.StatusCode}");
            }

            if (responseMessage.StatusCode == status)
            {
                string jsonString;

                if (responseMessage.Content.Headers.TryGetValues("Content-Encoding", out var contentEncoding))
                {
                    jsonString = await GetEncodingString(responseMessage, contentEncoding.ToArray());
                }
                else
                {
                    jsonString = await responseMessage.Content.ReadAsStringAsync();
                }

                return JsonSerializer.Deserialize<T>(jsonString, JsonSerializerOptions);
            }

            var message = await responseMessage.Content.ReadAsStringAsync();

            throw new Exception(
                $"Expected Status code {status} received {responseMessage.StatusCode}{Environment.NewLine}{message}");
        }

        private Task<string> GetEncodingString(HttpResponseMessage responseMessage, string[] contentEncoding)
        {
            if (contentEncoding.Contains("br"))
            {
                return BrEncodedString(responseMessage);
            }

            if (contentEncoding.Contains("gzip"))
            {
                return GzipEncodedString(responseMessage);
            }

            throw new Exception("Unknown encoding");
        }

        private async Task<string> GzipEncodedString(HttpResponseMessage responseMessage)
        {
            var responseStream = await responseMessage.Content.ReadAsStreamAsync();

            using (var brSteam = new GZipStream(responseStream, CompressionMode.Decompress))
            {
                using (var streamReader = new StreamReader(brSteam))
                {
                    return await streamReader.ReadToEndAsync();
                }
            }
        }

        private async Task<string> BrEncodedString(HttpResponseMessage responseMessage)
        {
            var responseStream = await responseMessage.Content.ReadAsStreamAsync();

            using (var brSteam = new BrotliStream(responseStream, CompressionMode.Decompress))
            {
                using (var streamReader = new StreamReader(brSteam))
                {
                    return await streamReader.ReadToEndAsync();
                }
            }
        }


        protected IClientSerializer CustomSerializer { get; set; }

        protected class InternalStartup
        {

        }

        public interface ISharedStorage
        {
            ConcurrentDictionary<string, object> Items { get; }
        }

        public class SharedStorage : ISharedStorage
        {
            public ConcurrentDictionary<string, object> Items { get; } = new ConcurrentDictionary<string, object>();
        }

        protected virtual JsonSerializerOptions JsonSerializerOptions => _jsonSerializerOptions ??=
            new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
    }
}
