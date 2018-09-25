using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using EasyRpc.AspNetCore;
using EasyRpc.AspNetCore.Converters;
using EasyRpc.AspNetCore.Messages;
using EasyRpc.AspNetCore.Middleware;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using SimpleFixture.xUnit;
using Xunit;

namespace EasyRpc.Tests.Middleware
{
    public class SerializeUseCaseTests : BaseRpcMiddlewareTests
    {
        public class OutOfOrderRequest
        {
            [JsonProperty(Order = 1, PropertyName = "jsonrpc")]
            public string JsonRpc { get; set; } = "2.0";

            [JsonProperty("params", Order = 2)]
            public object Parameters { get; set; }

            [JsonProperty(Order = 3)]
            public string Method { get; set; }

            [JsonProperty(Order = 4)]
            public int id { get; set; }
        }


        [Theory]
        [AutoData]
        public void SerializeListInOrderTest(IApplicationBuilder app, HttpContext context)
        {
            Configure(app, "/", api => { api.Expose<Service>().As("Service"); });

            var results = MakeCall<List<ResponseStrings>>(context, "/Service", "ServiceMethod", new
            {
                updates = new[]
                {
                    new TestMessage
                    {
                        QRN = "90E-2U-54",
                        FT = "89-TU",
                        ID = 9238718,
                        Approved = true,
                        NTR = "990231703",
                        Date = new DateTime(2018, 9, 25)
                    }
                }
            });

            Assert.NotNull(results);
            Assert.Single(results);

        }

        [Theory]
        [AutoData]
        public void SerializeListOutOfOrderTest(IApplicationBuilder app, HttpContext context)
        {
            var outOfOrder = new OutOfOrderRequest
            {
                Parameters = new
                {
                    updates = new[]
                    {
                        new TestMessage
                        {
                            QRN = "90E-2U-54",
                            FT = "89-TU",
                            ID = 9238718,
                            Approved = true,
                            NTR = "990231703",
                            Date = new DateTime(2018, 9, 25)
                        }
                    }
                },
                Method = "ServiceMethod",
                id = 1
            };

            Configure(app, "/", api => { api.Expose<Service>().As("Service"); });

            var results = MakeCall<List<ResponseStrings>>(context, "/Service", outOfOrder);

            Assert.NotNull(results);
            Assert.Single(results);

        }

        public class TestMessage
        {
            public string QRN { get; set; }

            public string FT { get; set; }

            public int ID { get; set; }

            public string NTR { get; set; }

            public DateTime Date { get; set; }

            public bool Approved { get; set; }
        }

        public class Service
        {
            public List<ResponseStrings> ServiceMethod(List<TestMessage> updates)
            {
                return updates.Select(t => new ResponseStrings { Response = t.FT + " " + t.Approved }).ToList();
            }
        }

        public class ResponseStrings
        {
            public string Response { get; set; }
        }
    }
}
