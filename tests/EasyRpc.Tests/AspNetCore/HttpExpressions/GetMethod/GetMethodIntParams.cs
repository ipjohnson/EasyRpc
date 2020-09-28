using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using EasyRpc.AspNetCore;
using EasyRpc.Tests.Services.Models;
using Xunit;

namespace EasyRpc.Tests.AspNetCore.HttpExpressions.GetMethod
{
    public class GetMethodIntParams : BaseRequestTest
    {
        #region Tests

        [Fact]
        public async Task HttpExpressions_GetMethod_OneInt()
        {
            var arg1 = 5;

            var response = await Get($"/OneInt/{arg1}");

            var value = await Deserialize<int>(response);

            Assert.Equal(arg1 , value);
        }

        [Fact]
        public async Task HttpExpressions_GetMethod_TwoInt()
        {
            var arg1 = 5;
            var arg2 = 10;

            var response = await Get($"/TwoInt/{arg1}/{arg2}");

            var value = await Deserialize<int>(response);

            Assert.Equal(arg1 + arg2, value);
        }

        [Fact]
        public async Task HttpExpressions_GetMethod_ThreeString()
        {
            var arg1 = 5;
            var arg2 = 10;
            var arg3 = 15;

            var response = await Get($"/ThreeInt/{arg1}/{arg2}/{arg3}");

            var value = await Deserialize<int>(response);

            Assert.Equal(arg1 + arg2 + arg3, value);
        }

        #endregion

        #region Registration
        protected override void ApiRegistration(IRpcApi api)
        {
            api.GetMethod("/OneInt/{arg1}", (int arg1) => arg1 );
            api.GetMethod("/TwoInt/{arg1}/{arg2}", (int arg1, int arg2) => arg1 + arg2);
            api.GetMethod("/ThreeInt/{arg1}/{arg2}/{arg3}", (int arg1, int arg2, int arg3) => arg1 + arg2 + arg3);

        }
        #endregion

    }
}
