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
    public class ParameterBodyDecimalTests : BaseRequestTest
    {
        #region Tests

        [Fact]
        public async Task ModelBinding_DefaultValues_ParameterBodyDecimalTest()
        {
            var response = await Post("/Service/AddValues", new { });

            var value = await Deserialize<GenericResult<decimal>>(response);

            Assert.NotNull(value);
            Assert.Equal(15, value.Result);
        }

        #endregion

        #region registration

        protected override void ApiRegistration(IApiConfiguration api)
        {
            api.DefaultHttpMethod(ExposeDefaultMethod.PostOnly);
            api.Expose<Service>();
        }

        #endregion

        #region Service

        public class Service
        {
            public decimal AddValues(decimal x = 5, decimal y = 10)
            {
                return x + y;
            }
        }

        #endregion
    }
}
