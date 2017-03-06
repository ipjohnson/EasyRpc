using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using EasyRpc.AspNetCore.Messages;
using EasyRpc.DynamicClient;
using EasyRpc.DynamicClient.ProxyGenerator;
using EasyRpc.Tests.Classes;
using EasyRpc.Tests.Middleware;
using EasyRPC.AspNetCore.Tests.Classes;
using Newtonsoft.Json.Linq;
using NSubstitute;
using SimpleFixture;
using SimpleFixture.NSubstitute;
using SimpleFixture.xUnit;
using Xunit;
using SimpleFixture.Attributes;

namespace EasyRpc.Tests.DynamicClient
{
    [SubFixtureInitialize]
    public class ProxyGeneratorTests
    {
        [Theory]
        [AutoData]
        [Export(typeof(JsonMethodObjectWriter))]
        [Export(typeof(DefaultNamingConventionService))]
        public void ProxyGenerator_Create_Interface_Proxy_ByOrder(IRpcProxyService proxyService,
                                                          ProxyGenerator proxyGenerator,
                                                          Fixture fixture)
        {
            byte[] bytes = new byte[0];

            proxyService.MakeCallWithReturn<int>(typeof(IIntMathService).Name, nameof(IIntMathService.Add), Arg.Any<byte[]>())
                .Returns(c =>
                {
                    bytes = c.Arg<byte[]>();

                    return 15;
                });

            var proxyType = proxyGenerator.GenerateProxyType(typeof(IIntMathService), false);

            var instance = (IIntMathService)fixture.Locate(proxyType);

            var value = instance.Add(5, 10);

            Assert.Equal(15, value);

            var request = bytes.Deserialize<RequestMessage>();

            Assert.NotNull(request);
            Assert.Equal("2.0", request.Version);
            Assert.Equal("Add", request.Method);
            Assert.False(string.IsNullOrEmpty(request.Id));

            var objectArray = request.Parameters as object[];

            Assert.NotNull(objectArray);
            Assert.Equal(2, objectArray.Length);
            Assert.Equal(5, Convert.ToInt32(objectArray[0]));
            Assert.Equal(10, Convert.ToInt32(objectArray[1]));
        }

        [Theory]
        [AutoData]
        [Export(typeof(JsonMethodObjectWriter))]
        [Export(typeof(DefaultNamingConventionService))]
        public void ProxyGenerator_Create_Interface_Proxy_ByName(IRpcProxyService proxyService,
                                                          ProxyGenerator proxyGenerator,
                                                          Fixture fixture)
        {
            byte[] bytes = new byte[0];

            proxyService.MakeCallWithReturn<int>(typeof(IIntMathService).Name, nameof(IIntMathService.Add), Arg.Any<byte[]>())
                .Returns(c =>
                 {
                     bytes = c.Arg<byte[]>();

                     return 15;
                 });

            var proxyType = proxyGenerator.GenerateProxyType(typeof(IIntMathService), true);

            var instance = (IIntMathService)fixture.Locate(proxyType);

            var value = instance.Add(5, 10);

            Assert.Equal(15, value);

            var request = bytes.Deserialize<RequestMessage>();

            Assert.NotNull(request);
            Assert.Equal("2.0", request.Version);
            Assert.Equal("Add", request.Method);
            Assert.False(string.IsNullOrEmpty(request.Id));

            var objectDictionary = request.Parameters as Dictionary<string, object>;

            Assert.NotNull(objectDictionary);
            Assert.Equal(2, objectDictionary.Count);
            Assert.Equal(5, Convert.ToInt32(objectDictionary["a"]));
            Assert.Equal(10, Convert.ToInt32(objectDictionary["b"]));
        }

        [Theory]
        [AutoData]
        [Export(typeof(JsonMethodObjectWriter))]
        [Export(typeof(DefaultNamingConventionService))]
        public void ProxyGenerator_ComplexObject(IRpcProxyService proxyService,
                                                          ProxyGenerator proxyGenerator,
                                                          Fixture fixture)
        {
            byte[] bytes = new byte[0];

            proxyService.MakeCallWithReturn<ResultObject>(typeof(IComplexService).Name, nameof(IComplexService.Add), Arg.Any<byte[]>())
                .Returns(c =>
                {
                    bytes = c.Arg<byte[]>();

                    return new ResultObject { Result = 15 };
                });

            var proxyType = proxyGenerator.GenerateProxyType(typeof(IComplexService), true);

            var instance = (IComplexService)fixture.Locate(proxyType);

            var value = instance.Add(new ComplexObject { A = 5, B = 10 });

            Assert.NotNull(value);
            Assert.Equal(15, value.Result);

            var request = bytes.Deserialize<RequestMessage>();

            Assert.NotNull(request);
            Assert.Equal("2.0", request.Version);
            Assert.Equal("Add", request.Method);
            Assert.False(string.IsNullOrEmpty(request.Id));

            var objectDictionary = request.Parameters as Dictionary<string, object>;

            Assert.NotNull(objectDictionary);
            Assert.Equal(1, objectDictionary.Count);

            var complex = objectDictionary["complex"] as JObject;

            Assert.NotNull(complex);
            Assert.Equal(5, complex["A"].ToObject(typeof(int)));
            Assert.Equal(10, complex["B"].ToObject(typeof(int)));
        }
    }
}
