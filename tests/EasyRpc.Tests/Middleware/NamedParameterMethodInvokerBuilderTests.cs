using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using EasyRpc.AspNetCore.Messages;
using EasyRpc.AspNetCore.Middleware;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SimpleFixture.NSubstitute;
using SimpleFixture.xUnit;
using Xunit;
using NSubstitute;

namespace EasyRpc.Tests.Middleware
{
    [SubFixtureInitialize]
    public class NamedParameterMethodInvokerBuilderTests
    {
        #region return void

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
        public void NamedParameterMethodInvokerBuilder_Void_Return(HttpContext context)
        {
            var invoker = new NamedParameterMethodInvokerBuilder();

            var method = typeof(TestClass).GetRuntimeMethod("Execute", new[] { typeof(int) });

            var parameterMethod = invoker.BuildInvokeMethodByNamedParameters(method);

            var testClass = new TestClass();

            var instance = parameterMethod("2.0", "id", testClass, new Dictionary<string, object> { { "a", 5 } }, context);

            instance.Wait();

            Assert.Equal(5, testClass.A);
        }

        [Theory]
        [AutoData]
        public void NamedParameterMethodInvokerBuilder_Task_Return(HttpContext context)
        {
            var invoker = new NamedParameterMethodInvokerBuilder();

            var method = typeof(TestClass).GetRuntimeMethod("AsyncExecute", new[] { typeof(int) });

            var parameterMethod = invoker.BuildInvokeMethodByNamedParameters(method);

            var testClass = new TestClass();

            var instance = parameterMethod("2.0", "id", testClass, new Dictionary<string, object> { { "a", 5 } }, context);

            instance.Wait();

            Assert.Equal(5, testClass.A);
        }
        #endregion

        #region Return value

        public interface IAdder
        {
            int Add(int a, int b);
        }

        public class Calculator
        {
            public int Add(int a, int b)
            {
                return a + b;
            }

            public async Task<int> AsyncAdd(int a, int b)
            {
                return a + b;
            }

            public int AddFromService([FromServices]IAdder adder, int a, int b)
            {
                return adder.Add(a, b);
            }

            public async Task<int> AsyncAddFromService([FromServices]IAdder adder, int a, int b)
            {
                return adder.Add(a, b);
            }
        }
        
        [Theory]
        [AutoData]
        public void NamedParameterMethodInvokerBuilder_Int_Return(HttpContext context)
        {
            var invoker = new NamedParameterMethodInvokerBuilder();

            var method = typeof(Calculator).GetRuntimeMethod("Add", new[] { typeof(int), typeof(int) });

            var parameterMethod = invoker.BuildInvokeMethodByNamedParameters(method);

            var calculator = new Calculator();

            var returnValueTask = parameterMethod("2.0", "id", calculator, new Dictionary<string, object> { { "a", 5 }, { "b", 10 } }, context);

            returnValueTask.Wait();

            var responseMessage = returnValueTask.Result as ResponseMessage<int>;

            Assert.NotNull(responseMessage);
            Assert.Equal(15, responseMessage.Result);
        }
        
        [Theory]
        [AutoData]
        public void NamedParameterMethodInvokerBuilder_TaskInt_Return(HttpContext context)
        {
            var invoker = new NamedParameterMethodInvokerBuilder();

            var method = typeof(Calculator).GetRuntimeMethod("AsyncAdd", new[] { typeof(int), typeof(int) });

            var parameterMethod = invoker.BuildInvokeMethodByNamedParameters(method);

            var calculator = new Calculator();

            var returnValueTask = parameterMethod("2.0", "id", calculator, new Dictionary<string, object> { { "a", 5 }, { "b", 10 } }, context);

            returnValueTask.Wait();

            var responseMessage = returnValueTask.Result as ResponseMessage<int>;

            Assert.NotNull(responseMessage);
            Assert.Equal(15, responseMessage.Result);
        }
        
        [Theory]
        [AutoData]
        public void NamedParameterMethodInvokerBuilder_FromService_Int_Return(HttpContext context, IAdder adder)
        {
            context.RequestServices.GetService(typeof(IAdder)).Returns(adder);

            adder.Add(5, 10).Returns(15);

            var invoker = new NamedParameterMethodInvokerBuilder();

            var method = typeof(Calculator).GetRuntimeMethod("AddFromService", new[] { typeof(IAdder), typeof(int), typeof(int) });

            var parameterMethod = invoker.BuildInvokeMethodByNamedParameters(method);

            var calculator = new Calculator();

            var returnValueTask = parameterMethod("2.0", "id", calculator, new Dictionary<string, object> { { "a", 5 }, { "b", 10 } }, context);

            returnValueTask.Wait();

            var responseMessage = returnValueTask.Result as ResponseMessage<int>;

            Assert.NotNull(responseMessage);
            Assert.Equal(15, responseMessage.Result);
        }
        
        [Theory]
        [AutoData]
        public void NamedParameterMethodInvokerBuilder_Task_FromService_Int_Return(HttpContext context, IAdder adder)
        {
            context.RequestServices.GetService(typeof(IAdder)).Returns(adder);

            adder.Add(5, 10).Returns(15);

            var invoker = new NamedParameterMethodInvokerBuilder();

            var method = typeof(Calculator).GetRuntimeMethod("AsyncAddFromService", new[] { typeof(IAdder), typeof(int), typeof(int) });

            var parameterMethod = invoker.BuildInvokeMethodByNamedParameters(method);

            var calculator = new Calculator();

            var returnValueTask = parameterMethod("2.0", "id", calculator, new Dictionary<string, object> { { "a", 5 }, { "b", 10 } }, context);

            returnValueTask.Wait();

            var responseMessage = returnValueTask.Result as ResponseMessage<int>;

            Assert.NotNull(responseMessage);
            Assert.Equal(15, responseMessage.Result);
        }

        #endregion


        #region Default Values

        public class StringCalculator
        {
            public string Add(string a, string b = null)
            {
                return a + b;
            }
        }
        
        [Theory]
        [AutoData]
        public void NamedParameterMethodInvokerBuilder_Default_String_Null(HttpContext context)
        {
            var invoker = new NamedParameterMethodInvokerBuilder();

            var method = typeof(StringCalculator).GetRuntimeMethod("Add", new[] { typeof(string), typeof(string) });

            var parameterMethod = invoker.BuildInvokeMethodByNamedParameters(method);

            var calculator = new StringCalculator();

            var returnValueTask = parameterMethod("2.0", "id", calculator, new Dictionary<string, object> { { "a", "Hello" } }, context);

            returnValueTask.Wait();

            var responseMessage = returnValueTask.Result as ResponseMessage<string>;

            Assert.NotNull(responseMessage);
            Assert.Equal("Hello", responseMessage.Result);
        }

        #endregion
    }
}
