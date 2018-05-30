using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using EasyRpc.AspNetCore;
using EasyRpc.AspNetCore.Messages;
using EasyRpc.AspNetCore.Middleware;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using NSubstitute;
using SimpleFixture.NSubstitute;
using Xunit;

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

            app.ApplicationServices.GetService(typeof(IJsonRpcMessageProcessor))
                .Returns(new JsonRpcMessageProcessor(Options.Create(configuration ?? new RpcServiceConfiguration()),
                    new JsonSerializerProvider(),
                    new NamedParameterToArrayDelegateProvider(), 
                    new OrderedParameterToArrayDelegateProvider(),
                    new ArrayMethodInvokerBuilder()
                    ));

            app.Use(Arg.Do<Func<RequestDelegate, RequestDelegate>>(func => executeDelegate = func));

            app.UseJsonRpc(route, api);

            MiddlewareContextInstance = new MiddlewareContext { ExecuteDelegate = executeDelegate };
        }
        

        protected T MakeCall<T>(HttpContext context, string route, string method, object values, string version = "2.0", string id = "1", bool compress = false)
        {
            var requestMessage = new RequestMessage { Version = version, Id = id, Method = method, Parameters = values };
            var responseStream = new MemoryStream();

            context.Request.Path = new PathString(route);
            context.Request.ContentType = "application/json";
            context.Request.Method = HttpMethod.Post.Method;

            context.Request.Body = requestMessage.SerializeToStream(compress);

            if (compress)
            {
                context.Request.Headers["Content-Encoding"] = new StringValues("gzip");
            }

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

                Assert.NotNull(response);
                Assert.NotNull((response as ErrorResponseMessage)?.Error);

                return response;
            }
            else
            {
                if (context.Response.Headers["Content-Encoding"].Contains("gzip"))
                {
                    var gzipStream = new GZipStream(new MemoryStream(responseStream.ToArray()), CompressionMode.Decompress);
                    
                    responseStream = new MemoryStream();
                    
                    gzipStream.CopyTo(responseStream);
                }

                var response = responseStream.DeserializeFromMemoryStream<ResponseMessage<T>>();

                Assert.NotNull(response);

                return response.Result;
            }
        }
    }
}
