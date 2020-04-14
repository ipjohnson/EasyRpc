using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using EasyRpc.AspNetCore;
using EasyRpc.Tests.Services.Models;
using Xunit;

namespace EasyRpc.Tests.AspNetCore.HttpExpressions.GetMethod
{
    public class GetMethodStringParams : BaseRequestTest
    {
        private string _postFix = " World";

        #region Tests

        [Fact]
        public async Task HttpExpressions_GetMethod_OneString()
        {
            var arg1 = "Hello";

            var response = await Get($"/OneString/{arg1}");

            var value = await Deserialize<GenericResult<string>>(response);

            Assert.NotNull(value);
            Assert.Equal(arg1 + _postFix, value.Result);
        }

        [Fact]
        public async Task HttpExpressions_GetMethod_TwoString()
        {
            var arg1 = "Hello";
            var arg2 = " big";

            var response = await Get($"/TwoString/{arg1}/{arg2}");

            var value = await Deserialize<GenericResult<string>>(response);

            Assert.NotNull(value);
            Assert.Equal(arg1 + arg2 + _postFix, value.Result);
        }

        [Fact]
        public async Task HttpExpressions_GetMethod_ThreeString()
        {
            var arg1 = "Hello";
            var arg2 = " big";
            var arg3 = " beautiful";

            var response = await Get($"/ThreeString/{arg1}/{arg2}/{arg3}");

            var value = await Deserialize<GenericResult<string>>(response);

            Assert.NotNull(value);
            Assert.Equal(arg1 + arg2 + arg3 + _postFix, value.Result);
        }

        #endregion

        #region Registration
        protected override void ApiRegistration(IApiConfiguration api)
        {
            api.GetMethod("/OneString/{arg1}", (string arg1) => arg1 + _postFix);
            api.GetMethod("/TwoString/{arg1}/{arg2}", (string arg1, string arg2) => arg1 + arg2 + _postFix);
            api.GetMethod("/ThreeString/{arg1}/{arg2}/{arg3}", (string arg1, string arg2, string arg3) => arg1 + arg2 + arg3 + _postFix);
        }
        #endregion

    }
}
