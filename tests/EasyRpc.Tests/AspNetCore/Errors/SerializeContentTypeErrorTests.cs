using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using EasyRpc.Abstractions.Path;
using EasyRpc.AspNetCore;
using EasyRpc.AspNetCore.Serializers;
using EasyRpc.DynamicClient.MessagePack;
using MessagePack;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace EasyRpc.Tests.AspNetCore.Errors
{
    public class SerializeContentTypeErrorTests : BaseRequestTest
    {
        public SerializeContentTypeErrorTests()
        {
            CustomSerializer = new MessagePackClientSerializer();
        }

        #region Service

        public class IntMath
        {

            [GetMethod]
            public PostReturn GetAdd(int x, int y)
            {
                return new PostReturn { Result = x + y };
            }
        }

        #endregion

        #region PostValue

        [MessagePackObject]
        public class PostReturn
        {
            [Key(0)]
            public int Result { get; set; }
        }

        #endregion

        #region Tests

       
        [Fact]
        public async Task Errors_ContentTypeNotFound_Get()
        {
            var response = await Get("/IntMath/GetAdd/5/10");

            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.UnsupportedMediaType, response.StatusCode);
        }

        #endregion

        #region registration

        /// <inheritdoc />
        protected override void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton(Shared);
            services.AddRpcServices(s => s.RegisterJsonSerializer = false);
            services.AddScoped<IContentSerializer, XmlContentSerializer>();
        }

        protected override void ApiRegistration(IApiConfiguration api)
        {
            api.Expose<IntMath>();
        }

        #endregion
    }
}
