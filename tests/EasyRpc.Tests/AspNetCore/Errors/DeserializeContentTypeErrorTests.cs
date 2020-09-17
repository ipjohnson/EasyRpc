using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using EasyRpc.Abstractions.Path;
using EasyRpc.AspNetCore;
using EasyRpc.AspNetCore.CodeGeneration;
using EasyRpc.AspNetCore.MessagePack;
using EasyRpc.AspNetCore.Serializers;
using EasyRpc.DynamicClient.MessagePack;
using EasyRpc.Tests.AspNetCore.Serialization.msgpack;
using EasyRpc.Tests.Services.SimpleServices;
using MessagePack;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace EasyRpc.Tests.AspNetCore.Errors
{
    public class DeserializeContentTypeErrorTests : BaseRequestTest
    {
        public DeserializeContentTypeErrorTests()
        {
            CustomSerializer = new MessagePackClientSerializer();
        }

        #region Service

        public class IntMath
        {
            public PostReturn Add(int x, int y)
            {
                return new PostReturn { Result = x + y };
            }

            [GetMethod]
            public PostReturn GetAdd(int x, int y)
            {
                return new PostReturn{ Result = x + y };
            }
        }

        #endregion

        #region PostValue

        [MessagePackObject]
        public class PostValue
        {
            [Key(0)]
            public int X { get; set; }

            [Key(1)]
            public int Y { get; set; }
        }

        [MessagePackObject]
        public class PostReturn
        {
            [Key(0)]
            public int Result { get; set; }
        }

        #endregion

        #region Tests

        [Fact]
        public async Task Errors_DeserializeContentTypeNotFound()
        {
            var postValue = new PostValue { X = 10, Y = 5 };

            var response = await Post("/IntMath/Add", postValue);

            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.UnsupportedMediaType, response.StatusCode);
        }

        #endregion

        #region registration

        protected override void ApiRegistration(IApiConfiguration api)
        {
            api.Expose<IntMath>();
        }

        #endregion
    }
}
