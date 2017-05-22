using System.Reflection;
using System.Threading.Tasks;
using EasyRpc.AspNetCore.Messages;
using EasyRpc.AspNetCore.Middleware;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using SimpleFixture.NSubstitute;
using SimpleFixture.xUnit;
using Xunit;

namespace EasyRpc.Tests.Middleware
{
    [SubFixtureInitialize]
    public class OrderedParameterMethodInvokeBuilderTests
    {
        #region No return value
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
        #endregion

        #region return value

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

            public async Task<int> TaskAdd(int a, int b)
            {
                return a + b;
            }

            public int AddFromService([FromServices] IAdder adder, int a, int b)
            {
                return adder.Add(a, b);
            }

            public async Task<int> TaskAddFromService([FromServices] IAdder adder, int a, int b)
            {
                return adder.Add(a, b);
            }

        }


        [Theory]
        [AutoData]
        public void OrderedParameterMethodInvokeBuilder_Return_Int(HttpContext context)
        {
            var invoker = new OrderedParameterMethodInvokeBuilder();

            var method = typeof(Calculator).GetRuntimeMethod("Add", new[] { typeof(int), typeof(int) });

            var parameterMethod = invoker.BuildInvokeMethodOrderedParameters(method);

            var calculator = new Calculator();

            var returnValueTask = parameterMethod("2.0", "id", calculator, new object[] { 5, 10 }, context);

            returnValueTask.Wait();

            var responseMessage = returnValueTask.Result as ResponseMessage<int>;

            Assert.NotNull(responseMessage);
            Assert.Equal(15, responseMessage.Result);
        }

        [Theory]
        [AutoData]
        public void OrderedParameterMethodInvokeBuilder_Return_Int_TaskAdd(HttpContext context)
        {
            var invoker = new OrderedParameterMethodInvokeBuilder();

            var method = typeof(Calculator).GetRuntimeMethod("TaskAdd", new[] { typeof(int), typeof(int) });

            var parameterMethod = invoker.BuildInvokeMethodOrderedParameters(method);

            var calculator = new Calculator();

            var returnValueTask = parameterMethod("2.0", "id", calculator, new object[] { 5, 10 }, context);

            returnValueTask.Wait();

            var responseMessage = returnValueTask.Result as ResponseMessage<int>;

            Assert.NotNull(responseMessage);
            Assert.Equal(15, responseMessage.Result);
        }

        [Theory]
        [AutoData]
        public void OrderedParameterMethodInvokeBuilder_Return_Int_FromService(HttpContext context, IAdder adder)
        {
            context.RequestServices.GetService(typeof(IAdder)).Returns(adder);

            adder.Add(5, 10).Returns(15);

            var invoker = new OrderedParameterMethodInvokeBuilder();

            var method = typeof(Calculator).GetRuntimeMethod("AddFromService", new[] { typeof(IAdder), typeof(int), typeof(int) });

            var parameterMethod = invoker.BuildInvokeMethodOrderedParameters(method);

            var calculator = new Calculator();

            var returnValueTask = parameterMethod("2.0", "id", calculator, new object[] { 5, 10 }, context);

            returnValueTask.Wait();

            var responseMessage = returnValueTask.Result as ResponseMessage<int>;

            Assert.NotNull(responseMessage);
            Assert.Equal(15, responseMessage.Result);
        }

        [Theory]
        [AutoData]
        public void OrderedParameterMethodInvokeBuilder_Return_TaskInt_FromService(HttpContext context, IAdder adder)
        {
            context.RequestServices.GetService(typeof(IAdder)).Returns(adder);

            adder.Add(5, 10).Returns(15);

            var invoker = new OrderedParameterMethodInvokeBuilder();

            var method = typeof(Calculator).GetRuntimeMethod("TaskAddFromService", new[] { typeof(IAdder), typeof(int), typeof(int) });

            var parameterMethod = invoker.BuildInvokeMethodOrderedParameters(method);

            var calculator = new Calculator();

            var returnValueTask = parameterMethod("2.0", "id", calculator, new object[] { 5, 10 }, context);

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
        public void OrderedParameterMethodInvokeBuilder_Default_String_Null(HttpContext context)
        {
            var invoker = new OrderedParameterMethodInvokeBuilder();

            var method = typeof(StringCalculator).GetRuntimeMethod("Add", new[] { typeof(string), typeof(string) });

            var parameterMethod = invoker.BuildInvokeMethodOrderedParameters(method);

            var calculator = new StringCalculator();

            var returnValueTask = parameterMethod("2.0", "id", calculator, new object[] { "Hello" }, context);

            returnValueTask.Wait();

            var responseMessage = returnValueTask.Result as ResponseMessage<string>;

            Assert.NotNull(responseMessage);
            Assert.Equal("Hello", responseMessage.Result);
        }

        #endregion
    }
}
