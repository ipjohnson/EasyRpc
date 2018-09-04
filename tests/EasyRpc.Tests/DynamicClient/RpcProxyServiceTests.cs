using System;
using System.Net;
using System.Net.Http;
using EasyRpc.AspNetCore.Messages;
using EasyRpc.DynamicClient;
using EasyRpc.DynamicClient.ProxyGenerator;
using EasyRpc.Tests.Middleware;
using NSubstitute;
using SimpleFixture.NSubstitute;
using SimpleFixture.xUnit;
using Xunit;
using JsonRpcErrorCode = EasyRpc.AspNetCore.Messages.JsonRpcErrorCode;

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

            clientProvider.GetHttpClient(Arg.Any<string>(), Arg.Any<string>()).Returns(client);

            var bytes = new byte[] { 0, 1, 1, 0 };

            proxyService.MakeCallNoReturn("SomeNamespace", "SomeClass", "SomeMethod", bytes, false, false);

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

            clientProvider.GetHttpClient(Arg.Any<string>(), Arg.Any<string>()).Returns(client);

            var bytes = new byte[] { 0, 1, 1, 0 };

            var intValue = proxyService.MakeCallWithReturn<int>("SomeNamespace", "SomeClass", "SomeMethod", bytes, false, false);

            Assert.Equal(10, intValue);

            client.Received().SendAsync(Arg.Is<HttpRequestMessage>(message => ValidateSomeClassSomeMethod(message)));
        }

        [Theory]
        [AutoData]
        public void RpcProxyServiceMakeCallMethodNotFound(IRpcHttpClient client,
            IRpcHttpClientProvider clientProvider,
            RpcProxyService proxyService)
        {
            var response = new HttpResponseMessage(HttpStatusCode.Accepted)
            {
                Content = new StreamContent(new ErrorResponseMessage(JsonRpcErrorCode.MethodNotFound, "Not Found").SerializeToStream())
            };

            client.SendAsync(Arg.Any<HttpRequestMessage>()).Returns(response);

            clientProvider.GetHttpClient(Arg.Any<string>(), Arg.Any<string>()).Returns(client);

            var bytes = new byte[] { 0, 1, 1, 0 };

            Assert.Throws<AggregateException>(() => proxyService.MakeCallNoReturn("SomeNamespace", "SomeClass", "SomeMethod", bytes, false, false));
        }

        [Theory]
        [AutoData]
        public void RpcProxyServiceMakeCallMethodNotAuthorized(IRpcHttpClient client,
            IRpcHttpClientProvider clientProvider,
            RpcProxyService proxyService)
        {
            var response = new HttpResponseMessage(HttpStatusCode.Accepted)
            {
                Content = new StreamContent(new ErrorResponseMessage( JsonRpcErrorCode.UnauthorizedAccess, "Not Authorized").SerializeToStream())
            };

            client.SendAsync(Arg.Any<HttpRequestMessage>()).Returns(response);

            clientProvider.GetHttpClient(Arg.Any<string>(), Arg.Any<string>()).Returns(client);

            var bytes = new byte[] { 0, 1, 1, 0 };

            Assert.Throws<AggregateException>(() => proxyService.MakeCallNoReturn("SomeNamespace", "SomeClass", "SomeMethod", bytes, false, false));
        }

        [Theory]
        [AutoData]
        public void RpcProxyServiceMakeCallInternalError(IRpcHttpClient client,
            IRpcHttpClientProvider clientProvider,
            RpcProxyService proxyService)
        {
            var response = new HttpResponseMessage(HttpStatusCode.Accepted)
            {
                Content = new StreamContent(new ErrorResponseMessage(JsonRpcErrorCode.InternalServerError, "Internal Server Error").SerializeToStream())
            };

            client.SendAsync(Arg.Any<HttpRequestMessage>()).Returns(response);

            clientProvider.GetHttpClient(Arg.Any<string>(), Arg.Any<string>()).Returns(client);

            var bytes = new byte[] { 0, 1, 1, 0 };

            Assert.Throws<AggregateException>(() => proxyService.MakeCallNoReturn("SomeNamespace", "SomeClass", "SomeMethod", bytes, false, false));
        }

        [Theory]
        [AutoData]
        public void RpcProxyServiceMakeCallReturnValueCompressResponse(IRpcHttpClient client,
            IRpcHttpClientProvider clientProvider,
            RpcProxyService proxyService)
        {
            var response = new HttpResponseMessage(HttpStatusCode.Accepted)
            {
                Content = new StreamContent(new ResponseMessage<int>(10, "2.0", "1").SerializeToStream("gzip"))
            };

            response.Content.Headers.Add("Content-Encoding", "gzip");

            client.SendAsync(Arg.Any<HttpRequestMessage>()).Returns(response);

            clientProvider.GetHttpClient(Arg.Any<string>(), Arg.Any<string>()).Returns(client);

            var bytes = new byte[] { 0, 1, 1, 0 };

            var intValue = proxyService.MakeCallWithReturn<int>("SomeNamespace", "SomeClass", "SomeMethod", bytes, false, true);

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
