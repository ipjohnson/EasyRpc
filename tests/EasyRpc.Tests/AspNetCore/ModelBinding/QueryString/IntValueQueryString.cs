using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using EasyRpc.AspNetCore;
using EasyRpc.Tests.Services.Models;
using Xunit;

namespace EasyRpc.Tests.AspNetCore.ModelBinding.QueryString
{
    public class IntValueQueryString : BaseRequestTest
    {
        #region Tests

        [Fact]
        public async Task ModelBinding_QueryString_IntValueQueryString()
        {
            var count = 12345;
            var response = await Get($"/test?count={count}");

            var value = await Deserialize<int>(response);

            Assert.Equal(count, value);
        }

        #endregion

        #region Registration

        protected override void ApiRegistration(IRpcApi api)
        {
            api.Method.Get("/test", (int count) => count);
        }

        #endregion
    }
}
