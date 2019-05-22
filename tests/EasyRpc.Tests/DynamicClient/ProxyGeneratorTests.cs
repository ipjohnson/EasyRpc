using System;
using System.Reflection;
using EasyRpc.DynamicClient;
using EasyRpc.DynamicClient.ProxyGenerator;
using EasyRpc.Tests.Classes;
using EasyRpc.Tests.Middleware;
using Newtonsoft.Json.Linq;
using NSubstitute;
using SimpleFixture;
using SimpleFixture.NSubstitute;
using SimpleFixture.xUnit;
using Xunit;
using SimpleFixture.Attributes;
using RpcRequestMessage = EasyRpc.DynamicClient.Messages.RpcRequestMessage;

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

            proxyService.MakeCallWithReturn<int>(typeof(IIntMathService).Namespace, typeof(IIntMathService).Name,
                    nameof(IIntMathService.Add), Arg.Any<byte[]>(), false, false)
                .Returns(c =>
                {
                    bytes = c.Arg<byte[]>();

                    return 15;
                });

            var proxyType = proxyGenerator.GenerateProxyType(typeof(IIntMathService), false);

            var instance = (IIntMathService)fixture.Locate(proxyType);

            var value = instance.Add(5, 10);

            Assert.Equal(15, value);

            var request = bytes.Deserialize<RpcRequestMessage>();

            Assert.NotNull(request);
            Assert.Equal("2.0", request.Version);
            Assert.Equal("Add", request.Method);
            Assert.False(string.IsNullOrEmpty(request.Id));

            var objectArray = ((JArray)request.Parameters).ToObject<object[]>();

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

            proxyService.MakeCallWithReturn<int>(typeof(IIntMathService).Namespace, typeof(IIntMathService).Name,
                    nameof(IIntMathService.Add), Arg.Any<byte[]>(), false, false)
                .Returns(c =>
                {
                    bytes = c.Arg<byte[]>();

                    return 15;
                });

            var proxyType = proxyGenerator.GenerateProxyType(typeof(IIntMathService), true);

            var instance = (IIntMathService)fixture.Locate(proxyType);

            var value = instance.Add(5, 10);

            Assert.Equal(15, value);

            var request = bytes.Deserialize<RpcRequestMessage>();

            Assert.NotNull(request);
            Assert.Equal("2.0", request.Version);
            Assert.Equal("Add", request.Method);
            Assert.False(string.IsNullOrEmpty(request.Id));

            // todo
            //var objectDictionary = request.Parameters as Dictionary<string, object>;

            //Assert.NotNull(objectDictionary);
            //Assert.Equal(2, objectDictionary.Count);
            //Assert.Equal(5, Convert.ToInt32(objectDictionary["a"]));
            //Assert.Equal(10, Convert.ToInt32(objectDictionary["b"]));
        }

        [Theory]
        [AutoData]
        [Export(typeof(JsonMethodObjectWriter))]
        [Export(typeof(DefaultNamingConventionService))]
        public void ProxyGenerator_Create_Interface_Proxy_ByName_Enum(IRpcProxyService proxyService,
            ProxyGenerator proxyGenerator,
            Fixture fixture)
        {
            byte[] bytes = new byte[0];

            proxyService.MakeCallWithReturn<TestEnum>(typeof(IEnumValueService).Namespace, typeof(IEnumValueService).Name, nameof(IEnumValueService.GetTestEnum), Arg.Any<byte[]>(), false, false)
                .Returns(c =>
                {
                    bytes = c.Arg<byte[]>();

                    return TestEnum.Value2;
                });

            var proxyType = proxyGenerator.GenerateProxyType(typeof(IEnumValueService), true);

            var instance = (IEnumValueService)fixture.Locate(proxyType);

            var value = instance.GetTestEnum(TestEnum.Value1);

            Assert.Equal(TestEnum.Value2, value);

            var request = bytes.Deserialize<RpcRequestMessage>();

            Assert.NotNull(request);
            Assert.Equal("2.0", request.Version);
            Assert.Equal("GetTestEnum", request.Method);
            Assert.False(string.IsNullOrEmpty(request.Id));
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

            proxyService.MakeCallWithReturn<ResultObject>(typeof(IComplexService).Namespace, typeof(IComplexService).Name, nameof(IComplexService.Add), Arg.Any<byte[]>(), false, false)
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

            var request = bytes.Deserialize<RpcRequestMessage>();

            Assert.NotNull(request);
            Assert.Equal("2.0", request.Version);
            Assert.Equal("Add", request.Method);
            Assert.False(string.IsNullOrEmpty(request.Id));

        }

        public interface IVoidReturnInterface
        {
            void VoidMethod(int a, int b);
        }

        [Theory]
        [AutoData]
        [Export(typeof(JsonMethodObjectWriter))]
        [Export(typeof(DefaultNamingConventionService))]
        public void ProxyGenerator_Void_Return(IRpcProxyService proxyService,
            ProxyGenerator proxyGenerator,
            Fixture fixture)
        {
            byte[] bytes = new byte[0];

            var proxyType = proxyGenerator.GenerateProxyType(typeof(IVoidReturnInterface), true);

            var instance = (IVoidReturnInterface)fixture.Locate(proxyType);

            instance.VoidMethod(10, 5);

            proxyService.Received(1).MakeCallNoReturn(typeof(IVoidReturnInterface).Namespace, typeof(IVoidReturnInterface).Name, nameof(IVoidReturnInterface.VoidMethod), Arg.Any<byte[]>(), false, false);
        }

        [Theory]
        [AutoData]
        [Export(typeof(JsonMethodObjectWriter))]
        [Export(typeof(DefaultNamingConventionService))]
        public void ProxyGenerator_Create_Interface_Proxy_Compress_Request(IRpcProxyService proxyService,
            IMethodCompressionPicker compressionPicker,
            ProxyGenerator proxyGenerator,
            Fixture fixture)
        {
            byte[] bytes = new byte[0];

            compressionPicker.CompressRequest(Arg.Any<MethodInfo>()).Returns(true);

            proxyService.MakeCallWithReturn<int>(typeof(IIntMathService).Namespace, typeof(IIntMathService).Name, nameof(IIntMathService.Add), Arg.Any<byte[]>(), true, false)
                .Returns(c =>
                {
                    bytes = c.Arg<byte[]>();

                    return 15;
                });

            var proxyType = proxyGenerator.GenerateProxyType(typeof(IIntMathService), true);

            var instance = (IIntMathService)fixture.Locate(proxyType);

            var value = instance.Add(5, 10);

            Assert.Equal(15, value);

            var request = bytes.DeserializeGzip<RpcRequestMessage>();

            Assert.NotNull(request);
            Assert.Equal("2.0", request.Version);
            Assert.Equal("Add", request.Method);
            Assert.False(string.IsNullOrEmpty(request.Id));

            //var objectDictionary = request.Parameters as Dictionary<string, object>;

            //Assert.NotNull(objectDictionary);
            //Assert.Equal(2, objectDictionary.Count);
            //Assert.Equal(5, Convert.ToInt32(objectDictionary["a"]));
            //Assert.Equal(10, Convert.ToInt32(objectDictionary["b"]));
        }


        [Theory]
        [AutoData]
        [Export(typeof(JsonMethodObjectWriter))]
        [Export(typeof(DefaultNamingConventionService))]
        public void ProxyGenerator_Create_Interface_Compress_Response(IRpcProxyService proxyService,
            IMethodCompressionPicker compressionPicker,
            ProxyGenerator proxyGenerator,
            Fixture fixture)
        {
            byte[] bytes = new byte[0];

            compressionPicker.CompressResponse(Arg.Any<MethodInfo>()).Returns(true);

            proxyService.MakeCallWithReturn<int>(typeof(IIntMathService).Namespace, typeof(IIntMathService).Name, nameof(IIntMathService.Add), Arg.Any<byte[]>(), false, true)
                .Returns(c =>
                {
                    bytes = c.Arg<byte[]>();

                    return 15;
                });

            var proxyType = proxyGenerator.GenerateProxyType(typeof(IIntMathService), true);

            var instance = (IIntMathService)fixture.Locate(proxyType);

            var value = instance.Add(5, 10);

            Assert.Equal(15, value);

            var request = bytes.Deserialize<RpcRequestMessage>();

            Assert.NotNull(request);
            Assert.Equal("2.0", request.Version);
            Assert.Equal("Add", request.Method);
            Assert.False(string.IsNullOrEmpty(request.Id));

            //var objectDictionary = request.Parameters as Dictionary<string, object>;

            //Assert.NotNull(objectDictionary);
            //Assert.Equal(2, objectDictionary.Count);
            //Assert.Equal(5, Convert.ToInt32(objectDictionary["a"]));
            //Assert.Equal(10, Convert.ToInt32(objectDictionary["b"]));
        }

    }
}
