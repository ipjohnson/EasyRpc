using System;
using System.Text;
using EasyRpc.AspNetCore.Middleware;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using SimpleFixture.Attributes;
using SimpleFixture.NSubstitute;
using SimpleFixture.xUnit;
using Xunit;

namespace EasyRpc.Tests.Middleware
{
    [SubFixtureInitialize]
    public class RpcHeaderContextTests
    {
        public class HeaderClass
        {
            public int IntValue { get; set; }

            public string StringValue { get; set; }
        }

        [Theory]
        [AutoData]
        public void RpcHeaderContext_GetValue([Locate]RpcHeaderContext headerContext, IHttpContextAccessor accessor, [Locate]HttpContext context)
        { 
            var encodedHeader =
                Convert.ToBase64String(
                    Encoding.UTF8.GetBytes(
                        JsonConvert.SerializeObject(new HeaderClass { IntValue = 5, StringValue = "Hello" })));

            accessor.HttpContext = context;
            context.Request.Headers["RpcContext-HeaderClass"] = encodedHeader;

            var headerValue = headerContext.GetValue<HeaderClass>();

            var secondInstance = headerContext.GetValue<HeaderClass>();

            Assert.Same(headerValue, secondInstance);
            Assert.Equal(5, headerValue.IntValue);
            Assert.Equal("Hello", headerValue.StringValue);
        }
        
        [Theory]
        [AutoData]
        public void RpcHeaderContext_GetValue_WithKey([Locate]RpcHeaderContext headerContext, IHttpContextAccessor accessor, [Locate]HttpContext context)
        {
            var encodedHeader =
                Convert.ToBase64String(
                    Encoding.UTF8.GetBytes(
                        JsonConvert.SerializeObject(new HeaderClass { IntValue = 5, StringValue = "Hello" })));

            accessor.HttpContext = context;
            context.Request.Headers["RpcContext-HeaderClassKey"] = encodedHeader;

            var headerValue = headerContext.GetValue<HeaderClass>("Key");

            var secondInstance = headerContext.GetValue<HeaderClass>("Key");

            Assert.Same(headerValue, secondInstance);
            Assert.Equal(5, headerValue.IntValue);
            Assert.Equal("Hello", headerValue.StringValue);
        }
        
        [Theory]
        [AutoData]
        public void RpcHeaderContext_SetValue([Locate]RpcHeaderContext headerContext, IHttpContextAccessor accessor, [Locate]HttpContext context)
        {
            accessor.HttpContext = context;
        
            headerContext.SetValue(new HeaderClass { IntValue = 5, StringValue = "Hello" });

            var encodedHeaderValue = context.Response.Headers["RpcContext-HeaderClass"];
            
            var deserializedObject =
                JsonConvert.DeserializeObject<HeaderClass>(Encoding.UTF8.GetString(Convert.FromBase64String(encodedHeaderValue)));

            Assert.NotNull(deserializedObject);
            Assert.Equal(5, deserializedObject.IntValue);
            Assert.Equal("Hello", deserializedObject.StringValue);
        }

        [Theory]
        [AutoData]
        public void RpcHeaderContext_SetValue_WithKey([Locate]RpcHeaderContext headerContext, IHttpContextAccessor accessor, [Locate]HttpContext context)
        {
            accessor.HttpContext = context;

            headerContext.SetValue(new HeaderClass { IntValue = 5, StringValue = "Hello" }, "Key");

            var encodedHeaderValue = context.Response.Headers["RpcContext-HeaderClassKey"];
            
            var deserializedObject =
                JsonConvert.DeserializeObject<HeaderClass>(Encoding.UTF8.GetString(Convert.FromBase64String(encodedHeaderValue)));

            Assert.NotNull(deserializedObject);
            Assert.Equal(5, deserializedObject.IntValue);
            Assert.Equal("Hello", deserializedObject.StringValue);
        }
    }
}
