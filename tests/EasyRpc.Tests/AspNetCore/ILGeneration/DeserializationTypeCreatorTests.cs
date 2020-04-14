using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using EasyRpc.AspNetCore;
using EasyRpc.AspNetCore.EndPoints;
using EasyRpc.AspNetCore.CodeGeneration;
using EasyRpc.AspNetCore.Configuration;
using EasyRpc.AspNetCore.Routing;
using Xunit;

namespace EasyRpc.Tests.AspNetCore.ILGeneration
{
    public class DeserializationTypeCreatorTests
    {
        //[Fact]
        public void CreateRequestParameter_ParameterNames()
        {
            var methodDefinition = CreateSimpleMethodDefinition(out var serializedInstance);

            var requestParameter = DeserializeMethodParameters(methodDefinition, serializedInstance);

            var parameterInfos = requestParameter.ParameterInfos.ToList();


            Assert.Equal(3, parameterInfos.Count);
            Assert.Equal("intValue", parameterInfos[0].Name);
            Assert.Equal("stringValue", parameterInfos[1].Name);
            Assert.Equal("nullableBoolValue", parameterInfos[2].Name);
            //Assert.Equal("stringValue", parameterNames[1]);
            //Assert.Equal("nullableBoolValue", parameterNames[2]);
        }

        //[Fact]
        public void CreateRequestParameter_ItemAccess()
        {
            var methodDefinition = CreateSimpleMethodDefinition(out var serializedInstance);

            var requestParameter = DeserializeMethodParameters(methodDefinition, serializedInstance);

            Assert.Equal(3, requestParameter.ParameterCount);
            Assert.Equal(5, requestParameter[0]);
            Assert.Equal("blah", requestParameter[1]);
            Assert.True((bool?)requestParameter[2]);

        }

        [Fact]
        public void CreateRequestParameter_Clone()
        {
            var methodDefinition = CreateSimpleMethodDefinition(out var serializedInstance);

            var requestParameter = DeserializeMethodParameters(methodDefinition, serializedInstance);

            var clone = requestParameter.Clone();

            Assert.Equal(3, requestParameter.ParameterCount);
            Assert.Equal(3, clone.ParameterCount);
            Assert.Equal(requestParameter[0], clone[0]);
            Assert.Equal(requestParameter[1], clone[1]);
            Assert.Equal(requestParameter[2], clone[2]);
        }

        public class ResultHolder
        {
            public int IntValue { get; set; }

            public string StringValue { get; set; }

            public bool? NullableBoolValue { get; set; }
        }

        public class Invoker
        {
            public ResultHolder InvokeMethod(int intValue, string stringValue, bool? nullableBoolValue)
            {
                return new ResultHolder { IntValue = intValue, StringValue = stringValue, NullableBoolValue = nullableBoolValue };
            }
        }

        //[Fact]
        //public async void InvokeTest()
        //{
        //    var methodDefinition = CreateSimpleMethodDefinition(out var serializedInstance);

        //    var creator = new DeserializationTypeCreator();
        //    var serializeType = creator.CreateTypeForMethod(methodDefinition);

        //    var methodInvokerService = new MethodInvokerCreationService(new WrappedResultTypeCreator());

        //    var invokeDelegate = methodInvokerService.BuildMethodInvoker(methodDefinition, serializeType);

        //    var context = new RequestExecutionContext(null, null, 200)
        //    {
        //        ServiceInstance = new Invoker(), 
        //        Parameters = (IRequestParameters)JsonSerializer.Deserialize(serializedInstance, serializeType)
        //    };

        //    await invokeDelegate(context);
        //}

        private static IRequestParameters DeserializeMethodParameters(EndPointMethodConfiguration methodDefinition,
            string serializedInstance)
        {
            var creator = new DeserializationTypeCreator();

            var serializeType = creator.CreateTypeForMethod(methodDefinition);

            return (IRequestParameters)JsonSerializer.Deserialize(serializedInstance, serializeType);
        }

        private static EndPointMethodConfiguration CreateSimpleMethodDefinition(out string serializedInstance)
        {
            var invokeInfo = new MethodInvokeInformation{ MethodToInvoke = typeof(Invoker).GetMethod("InvokeMethod")};
            var methodDefinition = new EndPointMethodConfiguration(new RpcRouteInformation(), null, invokeInfo, typeof(ResultHolder), null);

            methodDefinition.Parameters.Add(new RpcParameterInfo { Name = "intValue", ParamType = typeof(int), Position = 0 });
            methodDefinition.Parameters.Add(new RpcParameterInfo { Name = "stringValue", ParamType = typeof(string), Position = 1 });
            methodDefinition.Parameters.Add(new RpcParameterInfo { Name = "nullableBoolValue", ParamType = typeof(bool?), Position = 1 });

            serializedInstance = "{\"intValue\": 5, \"stringValue\":\"blah\", \"nullableBoolValue\": true}";

            return methodDefinition;
        }
    }
}
