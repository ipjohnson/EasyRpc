using System;
using System.Collections.Generic;
using System.Text;
using EasyRpc.AspNetCore;
using Microsoft.Extensions.DependencyInjection;
using SimpleFixture;
using SimpleFixture.Attributes;
using SimpleFixture.xUnit;
using Xunit;

namespace EasyRpc.Tests.Middleware
{
    public class AppBuilderExtensionsTests
    {
        [Theory]
        [AutoData]
        public void AppBuilderExtensions_AddJsonRpc(ServiceCollection serviceCollection)
        {
            serviceCollection.AddJsonRpc();

            Assert.Equal(9, serviceCollection.Count);
        }
    }
}
