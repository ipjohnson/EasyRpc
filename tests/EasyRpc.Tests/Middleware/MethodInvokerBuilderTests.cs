using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using EasyRpc.AspNetCore.Middleware;
using Microsoft.AspNetCore.Http;
using Xunit;

namespace EasyRpc.Tests.Middleware
{
    public class MethodInvokerBuilderTests
    {
        public class TestClass
        {
            public int Add(int a, int b)
            {
                return a + b;
            }
        }

        [Fact]
        public void BuildMethodInvoker()
        {
        }
    }
}
