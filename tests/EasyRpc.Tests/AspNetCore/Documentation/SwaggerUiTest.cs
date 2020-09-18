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
            /// <summary>
            /// Get value tests
            /// </summary>
            /// <param name="id">url id</param>
            /// <returns></returns>
            [GetMethod("/Service/GetValue/{id}")]
            public int GetValue(int id)
            {
                return 10;
            }

            /// <summary>
            /// Post test method
            /// </summary>
            /// <param name="id">id</param>
            /// <param name="model">model value</param>
            /// <returns></returns>
            [PostMethod("/Service/PostValue/{id}")]
            public Model PostValue(int id, Model model)
            {
                model.Value = id.ToString();

                return model;
            }

            public int AddValue(int x, int y)
            {
                return x + y;
            }
        }

        /// <summary>
        /// Model
        /// </summary>
        public class Model
        {
            /// <summary>
            /// Value
            /// </summary>
            public string Value { get; set; }
        }

        #endregion

        #region Tests

        [Theory]
        [InlineData("/swagger/index.html")]
        [InlineData("/swagger/swagger-ui.css")]
        [InlineData("/swagger/swagger-ui-bundle.js")]
        [InlineData("/swagger/swagger-ui-standalone-preset.js")]
        [InlineData("/swagger/api.json")]
        [InlineData("/swagger/api.json?OpenApi=2")]
        [InlineData("/swagger/swagger-ui.css", "br")]
        [InlineData("/swagger/swagger-ui-bundle.js", "br")]
        [InlineData("/swagger/swagger-ui-standalone-preset.js", "br")]
        public async Task Documentation_SwaggerUi(string path, string acceptEncoding = null)
        {
            AcceptEncoding = acceptEncoding;

            var response = await Get(path);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            if (acceptEncoding != null)
            {
                Assert.True(response.Content.Headers.ContentEncoding.Contains(acceptEncoding));
            }

            var bytes = await response.Content.ReadAsByteArrayAsync();

            Assert.True(bytes.Length > 0);
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
