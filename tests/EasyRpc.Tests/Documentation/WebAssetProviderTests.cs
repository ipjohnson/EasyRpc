using System;
using System.Collections.Generic;
using System.IO;
using EasyRpc.AspNetCore;
using EasyRpc.AspNetCore.Documentation;
using EasyRpc.AspNetCore.Middleware;
using Microsoft.AspNetCore.Http;
using SimpleFixture.NSubstitute;
using SimpleFixture.xUnit;
using Xunit;

namespace EasyRpc.Tests.Documentation
{
    [SubFixtureInitialize]
    public class WebAssetProviderTests
    {
        [Theory]
        [AutoData("/service-api/css/bundle.css")]
        public void BootstrapMinTest(WebAssetProvider provider, HttpContext context, string path)
        {
            var responseBody = new MemoryStream();

            context.Request.Path = path;
            context.Response.Body = responseBody;

            provider.Configure(new EndPointConfiguration("/service-api/", new Dictionary<string, IExposedMethodInformation>(), true, new DocumentationConfiguration()));

            if (!provider.ProcessRequest(context).Result)
            {
                throw new Exception("Asset not found");
            }

            var bytes = responseBody.ToArray();

            Assert.NotNull(bytes);
        }
    }
}
