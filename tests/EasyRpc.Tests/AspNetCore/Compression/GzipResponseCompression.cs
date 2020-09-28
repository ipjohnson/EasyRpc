using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using EasyRpc.Abstractions.Path;
using EasyRpc.AspNetCore;
using Xunit;

namespace EasyRpc.Tests.AspNetCore.Compression
{
    public class GzipResponseCompression : BaseRequestTest
    {
        #region Service

        public class Service
        {
            [GetMethod]
            public Task<List<Model>> GetModels()
            {
                return Task.FromResult(new List<Model>
                {
                    new Model {TestString = "Test Model1"},
                    new Model {TestString = "Test Model2"},
                    new Model {TestString = "Test Model3"},
                    new Model {TestString = "Test Model4"},
                    new Model {TestString = "Test Model5"},
                    new Model {TestString = "Test Model6"},
                    new Model {TestString = "Test Model7"},
                    new Model {TestString = "Test Model8"},
                    new Model {TestString = "Test Model9"}
                });
            }
        }

        public class Model
        {
            public string TestString { get; set; }
        }

        #endregion

        #region Tests

        [Fact]
        public async Task Compression_GzipResponse()
        {
            var response = await Get("/Service/GetModels");

            Assert.NotNull(response);
            Assert.False(response.Content.Headers.TryGetValues("Content-Encoding", out var contentEncoding));

            var result = await Deserialize<List<Model>>(response);

            Assert.NotNull(result);
            Assert.Equal(9, result.Count);

            AcceptEncoding = "gzip";

            response = await Get("/Service/GetModels");

            Assert.NotNull(response);
            Assert.True(response.Content.Headers.TryGetValues("Content-Encoding", out contentEncoding));
            Assert.Contains("gzip", contentEncoding);

            result = await Deserialize<List<Model>>(response);

            Assert.NotNull(result);
            Assert.Equal(9, result.Count);
        }

        #endregion

        #region registration

        protected override void ApiRegistration(IRpcApi api)
        {
            api.Configure.EnableCompression();
            api.Expose<Service>();
        }

        #endregion
    }
}
