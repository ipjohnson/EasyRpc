using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using EasyRpc.AspNetCore.Messages;
using EasyRpc.DynamicClient;
using EasyRpc.DynamicClient.ProxyGenerator;
using EasyRpc.Tests.Middleware;
using NSubstitute;
using NSubstitute.Core.Arguments;
using SimpleFixture.NSubstitute;
using SimpleFixture.xUnit;
using Xunit;

namespace EasyRpc.Tests.DynamicClient
{
    [SubFixtureInitialize]
    public class RpcProxyServiceTests
    {
        [Theory]
        [AutoData]
        public void RpcProxyServiceMakeCallNoReturn(IRpcHttpClient client,
                                                    IRpcHttpClientProvider clientProvider,
                                                    RpcProxyService proxyService)
        {
            var response = new HttpResponseMessage(HttpStatusCode.Accepted)
            {
                Content = new StreamContent(new EmptyResponseMessage("2.0", "1").SerializeToStream())
            };

            client.SendAsync(Arg.Any<HttpRequestMessage>()).Returns(response);

            clientProvider.GetHttpClient(Arg.Any<string>()).Returns(client);

            var bytes = new byte[] { 0, 1, 1, 0 };

            proxyService.MakeCallNoReturn("SomeClass", "SomeMethod", bytes);

            client.Received().SendAsync(Arg.Is<HttpRequestMessage>(message => ValidateSomeClassSomeMethod(message)));
        }

        [Theory]
        [AutoData]
        public void RpcProxyServiceMakeCallReturnValue(IRpcHttpClient client,
            IRpcHttpClientProvider clientProvider,
            RpcProxyService proxyService)
        {
            var response = new HttpResponseMessage(HttpStatusCode.Accepted)
            {
                Content = new StreamContent(new ResponseMessage<int>(10, "2.0", "1").SerializeToStream())
            };

            client.SendAsync(Arg.Any<HttpRequestMessage>()).Returns(response);

            clientProvider.GetHttpClient(Arg.Any<string>()).Returns(client);

            var bytes = new byte[] { 0, 1, 1, 0 };

            var intValue = proxyService.MakeCallWithReturn<int>("SomeClass", "SomeMethod", bytes);

            Assert.Equal(10, intValue);

            client.Received().SendAsync(Arg.Is<HttpRequestMessage>(message => ValidateSomeClassSomeMethod(message)));
        }

        private bool ValidateSomeClassSomeMethod(HttpRequestMessage message)
        {
            var data = message.Content.ReadAsByteArrayAsync().Result;

            Assert.Equal(0, data[0]);
            Assert.Equal(1, data[1]);
            Assert.Equal(1, data[2]);
            Assert.Equal(0, data[3]);

            return true;
        }
    }
}
