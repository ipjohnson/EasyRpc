using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using EasyRpc.AspNetCore;
using Xunit;

namespace EasyRpc.Tests.AspNetCore.Modules
{
    public class ModuleBasicTest : BaseRequestTest
    {
        #region Module

        public class TestModule : RpcModule
        {
            protected override void Configure(IRpcApi api)
            {
                api.GetMethod("/Service/All", () => new TestValue { Value = "Test" });
            }
        }

        public class TestValue
        {
            public string Value { get; set; }
        }

        #endregion
        
        #region Tests

        [Fact]
        public async Task Modules_BasicTest()
        {
            var response = await Get("/Service/All");

            var result = await Deserialize<TestValue>(response);

            Assert.NotNull(result);
            Assert.Equal("Test", result.Value);
        }

        #endregion

        #region registration

        protected override void ApiRegistration(IRpcApi api)
        {
            api.ExposeModules(new[] { typeof(TestModule) });
        }

        #endregion
    }
}
