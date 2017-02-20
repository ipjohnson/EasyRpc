using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using EasyRpc.AspNetCore.Messages;
using EasyRpc.AspNetCore.Middleware;
using EasyRPC.AspNetCore.Tests.Classes;
using Microsoft.AspNetCore.Http;
using SimpleFixture.NSubstitute;
using SimpleFixture.xUnit;
using Xunit;

namespace EasyRpc.Tests.Middleware
{
    [SubFixtureInitialize]
    public class OrderedParameterMethodInvokeBuilderTests
    {
        public class TestClass
        {
            public void Execute(int a)
            {
                A = a;
            }

            public async Task AsyncExecute(int a)
            {
                A = a;
            }

            public int A { get; set; }
        }
        
        [Theory]
        [AutoData]
        public void OrderedParameterMethodInvokeBuilder_ReturnVoid(HttpContext context)
        {
            var invoker = new OrderedParameterMethodInvokeBuilder();

            var method = typeof(TestClass).GetRuntimeMethod("Execute", new[] { typeof(int) });

            var parameterMethod = invoker.BuildInvokeMethodOrderedParameters(method);

            var testClass = new TestClass();

            var instance = parameterMethod("2.0", "id", testClass, new object[] { 5 }, context);

            instance.Wait();

            Assert.Equal(5, testClass.A);
        }

        [Theory]
        [AutoData]
        public void OrderedParameterMethodInvokeBuilder_ReturnInt(HttpContext context)
        {
            var invoker = new OrderedParameterMethodInvokeBuilder();

            var method = typeof(IntMathService).GetRuntimeMethod("Add", new[] { typeof(int), typeof(int) });

            var parameterMethod = invoker.BuildInvokeMethodOrderedParameters(method);

            var instance = parameterMethod("2.0", "id", new IntMathService(), new object[] { 2, 3 }, context);

            instance.Wait();

            var response = instance.Result as ResponseMessage<int>;

            Assert.NotNull(response);
            Assert.Equal(5, response.Result);
        }
        
        [Theory]
        [AutoData]
        public void OrderedParameterMethodInvokeBuilder_ReturnTaskInt(HttpContext context)
        {
            var invoker = new OrderedParameterMethodInvokeBuilder();

            var method = typeof(IntMathService).GetRuntimeMethod("AsyncAdd", new[] { typeof(int), typeof(int) });

            var parameterMethod = invoker.BuildInvokeMethodOrderedParameters(method);

            var instance = parameterMethod("2.0", "id", new IntMathService(), new object[] { 2, 3 }, context);

            instance.Wait();

            var response = instance.Result as ResponseMessage<int>;

            Assert.NotNull(response);
            Assert.Equal(5, response.Result);
        }
        
        [Theory]
        [AutoData]
        public void OrderedParameterMethodInvokeBuilder_ReturnTask(HttpContext context)
        {
            var invoker = new OrderedParameterMethodInvokeBuilder();

            var method = typeof(TestClass).GetRuntimeMethod("AsyncExecute", new[] { typeof(int) });

            var parameterMethod = invoker.BuildInvokeMethodOrderedParameters(method);

            var testClass = new TestClass();

            var instance = parameterMethod("2.0", "id", testClass, new object[] { 5 }, context);

            instance.Wait();

            Assert.Equal(5, testClass.A);
        }
    }
}
