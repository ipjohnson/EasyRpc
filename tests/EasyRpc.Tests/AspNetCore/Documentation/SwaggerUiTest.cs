using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using EasyRpc.Abstractions.Path;
using EasyRpc.AspNetCore;
using Xunit;

namespace EasyRpc.Tests.AspNetCore.Documentation
{
    public class SwaggerUiTest : BaseRequestTest
    {
        #region Service

        public class Service
        {
            [GetMethod]
            public int GetValue()
            {
                return 10;
            }
        }

        #endregion


        #region Tests

        [Theory]
        [InlineData("/swagger/index.html")]
        [InlineData("/swagger/swagger-ui.css")]
        [InlineData("/swagger/swagger-ui-bundle.js")]
        [InlineData("/swagger/swagger-ui-standalone-preset.js")]
        [InlineData("/swagger/api.json")]
        public async Task Documentation_SwaggerUi(string path)
        {
            var response = await Get(path);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var result = await response.Content.ReadAsStringAsync();

            Assert.True(result.Length > 0);
        }

        #endregion

        #region registration

        protected override void ApiRegistration(IApiConfiguration api)
        {
            api.Expose<Service>();
        }

        #endregion
    }
}
