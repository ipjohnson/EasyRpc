using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using EasyRpc.DynamicClient.CodeGeneration;
using EasyRpc.DynamicClient.ExecutionService;
using EasyRpc.DynamicClient.Serializers;
using Grace.Data;
using NSubstitute;
using NSubstitute.Core.Arguments;
using SimpleFixture;
using SimpleFixture.Attributes;
using SimpleFixture.NSubstitute;
using SimpleFixture.xUnit;
using Xunit;

namespace EasyRpc.Tests.DynamicClient.CodeGeneration.ServiceImplementationGeneratorTests
{
    public class TaskWithValueInterfaceReturn
    {
        #region Interface

        public interface ITestInterface
        {
            Task<int> Add(int a, int b);
        }

        #endregion


        [Theory]
        [AutoData]
        [Export(typeof(SerializationTypeCreator))]
        [SubFixtureInitialize]
        public async Task TaskWithValueInterfaceReturnTest(ServiceImplementationGenerator implementationGenerator, IClientSerializer clientSerializer, Fixture fixture)
        {
            var request = new ImplementationRequest
            {
                InterfaceType = typeof(ITestInterface),
                SingleParameterToBody = true,
                DefaultSerializer = clientSerializer
            };

            var type = implementationGenerator.GenerateImplementationForInterface(request);

            var testInterface = (ITestInterface)fixture.Locate(type);

            var a = 5;
            var b = 5;

            fixture.Locate<IRpcExecutionService>().ExecuteMethodWithValue<int>(Arg.Any<RpcExecuteInformation>(),
                Arg.Any<string>(), Arg.Any<object>(), Arg.Any<CancellationToken?>()).Returns(callInfo =>
            {
                var value = callInfo.ArgAt<object>(2);

                var properties = ReflectionService.GetPropertiesFromObject(value);

                var aValue = (int)properties[nameof(a)];
                var bValue = (int)properties[nameof(b)];

                return aValue + bValue;
            });


            var value = await testInterface.Add(5, 5);
            

            Assert.Equal(a + b, value);
        }
    }
}
