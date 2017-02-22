using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using EasyRpc.AspNetCore;
using EasyRpc.AspNetCore.Messages;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using NSubstitute;
using SimpleFixture.NSubstitute;
using Xunit;

namespace EasyRpc.Tests.Middleware
{
    [SubFixtureInitialize]
    public class BaseRpcMiddlewareTests
    {
        private class MiddlewareContext
        {
            public Func<RequestDelegate, RequestDelegate> ExecuteDelegate { get; set; }
        }

        private MiddlewareContext _middlewareContext;

        protected void Configure(IApplicationBuilder app, string route, Action<IApiConfiguration> api)
        {
            Func<RequestDelegate, RequestDelegate> executeDelegate = null;

            app.Use(Arg.Do<Func<RequestDelegate, RequestDelegate>>(func => executeDelegate = func));

            app.UseJsonRpc(route, api);

            _middlewareContext = new MiddlewareContext { ExecuteDelegate = executeDelegate };
        }

        protected T MakeCall<T>(HttpContext context, string route, string method, object values, string version = "2.0", string id = "1")
        {
            var requestMessage = new RequestMessage { Version = version, Id = id, Method = method, Parameters = values };
            var responseStream = new MemoryStream();

            context.Request.Path = new PathString(route);
            context.Request.ContentType = "application/json";
            context.Request.Body = requestMessage.SerializeToStream();

            context.Response.Body = responseStream;

            var result = _middlewareContext.ExecuteDelegate(httpContext => Task.CompletedTask);

            var taskResult = result(context);

            taskResult.Wait(1000);

            if (!taskResult.IsCompleted)
            {
                throw new Exception("Task never completed");
            }

            var response = responseStream.DeserializeFromMemoryStream<ResponseMessage<T>>();

            Assert.NotNull(response);

            return response.Result;
        }
    }
    public static class SerializeMethods
    {
        public static Stream SerializeToStream<T>(this T value)
        {
            MemoryStream returnStream = new MemoryStream();

            using (var text = new StreamWriter(returnStream))
            {
                using (var jsonStream = new JsonTextWriter(text))
                {
                    var serializer = new JsonSerializer();

                    serializer.Serialize(jsonStream, value);
                }
            }

            return new MemoryStream(returnStream.ToArray());
        }

        public static T DeserializeFromMemoryStream<T>(this MemoryStream stream)
        {
            var newMemoryStream = new MemoryStream(stream.ToArray());

            using (var text = new StreamReader(newMemoryStream))
            {
                using (var jsonStream = new JsonTextReader(text))
                {
                    var serializer = new JsonSerializer();

                    return serializer.Deserialize<T>(jsonStream);
                }
            }
        }
    }
}
