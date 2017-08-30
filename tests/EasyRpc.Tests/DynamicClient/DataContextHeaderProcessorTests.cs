using System.Linq;
using System.Net.Http;
using EasyRpc.DynamicClient;
using EasyRpc.Tests.Middleware;
using SimpleFixture.xUnit;
using Xunit;

namespace EasyRpc.Tests.DynamicClient
{
    public class DataClass
    {
        public int IntValue { get; set; }

        public string StringValue { get; set; }
    }

    public class DataContextHeaderProcessorTests
    {
        [Theory]
        [AutoData]
        public void DataContextHeaderProcessor_RequestMessage(DataContextHeaderProcessor processor, HttpRequestMessage message)
        {
            var data = new DataClass { IntValue = 5, StringValue = "StringValue" };

            processor.SetValue(data);

            processor.ProcessRequestHeader(message);

            var stringValues = message.Headers.GetValues("RpcContext-" + nameof(DataClass));

            var encodedValue = stringValues.First();

            var newData = encodedValue.DeserializeFromBase64String<DataClass>();

            Assert.NotNull(newData);
            Assert.Equal(5, newData.IntValue);
            Assert.Equal("StringValue", newData.StringValue);
        }
        
        [Theory]
        [AutoData]
        public void DataContextHeaderProcessor_ResponseMessage(DataContextHeaderProcessor processor,
            HttpResponseMessage message)
        {
            var serializedValue = new DataClass { IntValue = 5, StringValue = "StringValue" }.SerializeToBase64String();

            message.Headers.Add("RpcContext-" + nameof(DataClass), new[] { serializedValue });

            processor.ProcessResponseHeader(message);

            var value = processor.GetValue<DataClass>();

            Assert.NotNull(value);
            Assert.Equal(5, value.IntValue);
            Assert.Equal("StringValue", value.StringValue);
        }
    }
}
