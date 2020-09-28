using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using EasyRpc.Abstractions.Path;
using EasyRpc.AspNetCore;
using EasyRpc.Tests.Services.Models;
using Xunit;

namespace EasyRpc.Tests.AspNetCore.ModelBinding.DefaultValues
{
    public class ParameterBodyIntTests : BaseRequestTest
    {
        #region Tests

        [Fact]
        public async Task ModelBinding_DefaultValues_ParameterBody_IntTest()
        {
            var response = await Post("/Service/AddValues", new { });

            var value = await Deserialize<int>(response);

            Assert.Equal(15, value);
        }

        #endregion

        #region registration

        protected override void ApiRegistration(IRpcApi api)
        {
            api.DefaultHttpMethod(ExposeDefaultMethod.PostOnly);
            api.Expose<Service>();
        }

        #endregion

        #region Service

        public class Service
        {
            public int AddValues(int x = 5, int y = 10)
            {
                return x + y;
            }
        }

        #endregion
    }
}
