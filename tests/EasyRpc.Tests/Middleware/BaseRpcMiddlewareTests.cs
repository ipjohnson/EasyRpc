using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using EasyRpc.AspNetCore;
using EasyRpc.AspNetCore.Brotli;
using EasyRpc.AspNetCore.Content;
using EasyRpc.AspNetCore.Converters;
using EasyRpc.AspNetCore.Messages;
using EasyRpc.AspNetCore.Middleware;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using NSubstitute;
using SimpleFixture.NSubstitute;
using Xunit;
using RpcRequestMessage = EasyRpc.DynamicClient.Messages.RpcRequestMessage;

namespace EasyRpc.Tests.Middleware
{
    [SubFixtureInitialize]
    public class BaseRpcMiddlewareTests
    {
        protected class MiddlewareContext
        {
            public Func<RequestDelegate, RequestDelegate> ExecuteDelegate { get; set; }
        }

        protected MiddlewareContext MiddlewareContextInstance;

        protected void Configure(IApplicationBuilder app, string route, Action<IApiConfiguration> api, RpcServiceConfiguration configuration = null)
        {
            Func<RequestDelegate, RequestDelegate> executeDelegate = null;
            var options = Options.Create(configuration ?? new RpcServiceConfiguration());
            var fromService = new FromServicesManager(options);
            app.ApplicationServices.GetService(typeof(IOptions<RpcServiceConfiguration>)).Returns(options);
            app.ApplicationServices.GetService(typeof(IRpcMessageProcessor))
                .Returns(new RpcMessageProcessor(options,
                    new ContentEncodingProvider(new IContentEncoder[]{ new GzipContentEncoder(), new BrotliContentEncoder() }),
                    new ContentSerializerProvider(new IContentSerializer[] { new DefaultJsonContentSerializer(new ParameterArrayDeserializerBuilder(fromService), new NamedParameterDeserializerBuilder(fromService)) }),
                    new ExposeMethodInformationCacheManager(), 
                    new InstanceActivator()
                    ));

            app.ApplicationServices.GetService(typeof(IInstanceActivator)).Returns(new InstanceActivator());
            app.ApplicationServices.GetService(typeof(IArrayMethodInvokerBuilder))
                .Returns(new ArrayMethodInvokerBuilder());

            app.Use(Arg.Do<Func<RequestDelegate, RequestDelegate>>(func => executeDelegate = func));

            app.UseJsonRpc(route, api);

            MiddlewareContextInstance = new MiddlewareContext { ExecuteDelegate = executeDelegate };
        }

        protected T MakeCall<T>(HttpContext context, string route, string method, object values, string version = "2.0",
            string id = "1", string compressRequest = null, string compressResponse = null)
        {
            return MakeCall<T>(context, route,
                new RpcRequestMessage {Version = version, Method = method, Parameters = values, Id = id}, compressRequest, compressResponse);
        }

        protected T MakeCall<T>(HttpContext context, string route, object requestMessage, string compressRequest = null, string compressResponse = null)
        {
            var responseStream = new MemoryStream();

            context.Request.Headers.Returns(new HeaderDictionary());
            context.Request.Path = new PathString(route);
            context.Request.ContentType = "application/json";
            context.Request.Method = HttpMethod.Post.Method;

            context.Request.Body = requestMessage.SerializeToStream(compressRequest);

            if (compressRequest != null)
            {
                context.Request.Headers["Content-Encoding"] = new StringValues(compressRequest);
            }

            if (compressResponse != null)
            {
                context.Request.Headers["Accept-Encoding"] = new StringValues(compressResponse);
            }

            context.Response.Headers.Returns(new HeaderDictionary());
            context.Response.Body = responseStream;

            var result = MiddlewareContextInstance.ExecuteDelegate(httpContext => Task.CompletedTask);

            var taskResult = result(context);

            taskResult.Wait(1000);

            if (!taskResult.IsCompleted)
            {
                throw new Exception("Task never completed");
            }

            if (typeof(T) == typeof(ErrorResponseMessage))
            {
                var response = responseStream.DeserializeFromMemoryStream<T>();

                Assert.NotNull((response as ErrorResponseMessage)?.Error);

                return response;
            }
            else
            {
                if (context.Response.Headers["Content-Encoding"].Contains("gzip"))
                {
                    var compressStream = new GZipStream(new MemoryStream(responseStream.ToArray()), CompressionMode.Decompress);

                    responseStream = new MemoryStream();

                    compressStream.CopyTo(responseStream);
                }
                else if(context.Response.Headers["Content-Encoding"].Contains("br"))
                {
                    var compressStream = new BrotliStream(new MemoryStream(responseStream.ToArray()), CompressionMode.Decompress);

                    responseStream = new MemoryStream();

                    compressStream.CopyTo(responseStream);
                }

                var response = responseStream.DeserializeFromMemoryStream<ResponseMessage<T>>();

                Assert.NotNull(response);

                return response.Result;
            }
        }
    }
}
