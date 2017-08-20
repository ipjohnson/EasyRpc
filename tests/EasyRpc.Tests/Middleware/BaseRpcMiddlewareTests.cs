using System;
using System.IO;
using System.Threading.Tasks;
using EasyRpc.AspNetCore;
using EasyRpc.AspNetCore.Messages;
using EasyRpc.AspNetCore.Middleware;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
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

        protected void Configure(IApplicationBuilder app, string route, Action<IApiConfiguration> api)
        {
            Func<RequestDelegate, RequestDelegate> executeDelegate = null;

            app.ApplicationServices.GetService(typeof(IJsonRpcMessageProcessor))
                .Returns(new JsonRpcMessageProcessor(new JsonSerializerProvider(),
                    new OrderedParameterMethodInvokeBuilder(), new NamedParameterMethodInvokerBuilder(),
                    Options.Create(new RpcServiceConfiguration())));

            app.Use(Arg.Do<Func<RequestDelegate, RequestDelegate>>(func => executeDelegate = func));

            app.UseJsonRpc(route, api);

            MiddlewareContextInstance = new MiddlewareContext { ExecuteDelegate = executeDelegate };
        }

        protected T MakeCall<T>(HttpContext context, string route, string method, object values, string version = "2.0", string id = "1")
        {
            var requestMessage = new RequestMessage { Version = version, Id = id, Method = method, Parameters = values };
            var responseStream = new MemoryStream();

            context.Request.Path = new PathString(route);
            context.Request.ContentType = "application/json";
            context.Request.Body = requestMessage.SerializeToStream();

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
                var response = responseStream.DeserializeFromMemoryStream<ResponseMessage<T>>();

                Assert.NotNull(response);

                return response.Result;
            }
        }
    }

}
