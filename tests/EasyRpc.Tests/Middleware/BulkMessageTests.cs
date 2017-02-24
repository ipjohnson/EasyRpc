using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using EasyRpc.AspNetCore.Messages;
using EasyRPC.AspNetCore.Tests.Classes;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SimpleFixture.xUnit;
using Xunit;

namespace EasyRpc.Tests.Middleware
{
    public class ErrorClass
    {
        [JsonProperty("code")]
        public int Code { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }
    }

    public class RpcResponseMessage
    {
        [JsonProperty("jsonrpc")]
        public string Version { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("result")]
        public JToken Result { get; set; }

        [JsonProperty("error")]
        public ErrorClass Error { get; set; }
    }

    public class BulkMessageTests : BaseRpcMiddlewareTests
    {
        [Theory]
        [AutoData]
        public void Bulk_Send_Success(IApplicationBuilder app, HttpContext context)
        {
            Configure(app, "RpcApi", api =>
            {
                api.Expose<IntMathService>().As("IntMath");
            });

            var response = SendBulkMessages(context, "/RpcApi/IntMath",
                new RequestMessage { Version = "2.0", Id = "1", Method = "Add", Parameters = new { a = 5, b = 10 } },
                new RequestMessage { Version = "2.0", Id = "2", Method = "Add", Parameters = new { a = 10, b = 10 } },
                new RequestMessage { Version = "2.0", Id = "3", Method = "Add", Parameters = new { a = 15, b = 10 } });

            Assert.NotNull(response);
            Assert.Equal(3, response.Length);

            Assert.Equal("1", response[0].Id);
            Assert.Equal("2", response[1].Id);
            Assert.Equal("3", response[2].Id);

            Assert.Equal(15, response[0].Result.ToObject<int>());
            Assert.Equal(20, response[1].Result.ToObject<int>());
            Assert.Equal(25, response[2].Result.ToObject<int>());
        }

        public RpcResponseMessage[] SendBulkMessages(HttpContext context, string route, params RequestMessage[] requests)
        {
            var responseStream = new MemoryStream();

            context.Request.Path = new PathString(route);
            context.Request.ContentType = "application/json";
            context.Request.Body = requests.SerializeToStream();

            context.Response.Body = responseStream;

            var result = _middlewareContext.ExecuteDelegate(httpContext => Task.CompletedTask);

            var taskResult = result(context);

            taskResult.Wait(5000);

            if (!taskResult.IsCompleted)
            {
                throw new Exception("Task never completed");
            }

            var response = responseStream.DeserializeFromMemoryStream<RpcResponseMessage[]>();

            Assert.NotNull(response);

            return response;
        }
    }
}
