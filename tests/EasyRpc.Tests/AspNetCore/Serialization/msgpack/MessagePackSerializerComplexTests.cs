using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using EasyRpc.AspNetCore;
using EasyRpc.AspNetCore.CodeGeneration;
using EasyRpc.AspNetCore.MessagePack;
using EasyRpc.AspNetCore.Serializers;
using EasyRpc.DynamicClient.MessagePack;
using MessagePack;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace EasyRpc.Tests.AspNetCore.Serialization.msgpack
{
    public class MessagePackSerializerComplexTests : BaseRequestTest
    {
        public MessagePackSerializerComplexTests()
        {
            CustomSerializer = new MessagePackClientSerializer();
        }

        #region Service

        public class IntMath
        {
            public PostReturn Add(int x, int y)
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
        public async Task Serialization_MessagePack_Complex()
        {
            var postValue = new MessagePackSerializerSimpleTests.PostValue { X = 10, Y = 5 };

            var response = await Post("/IntMath/Add", postValue);

            var result = await Deserialize<PostReturn>(response);

            Assert.NotNull(result);
            Assert.Equal(postValue.X + postValue.Y, result.Result);
        }

        #endregion

        #region Configure

        /// <inheritdoc />
        protected override void ConfigureServices(IServiceCollection services)
        {
            base.ConfigureServices(services);

            services.AddTransient<ISerializationTypeAttributor, MessagePackSerializationTypeAttributor>();
            services.AddTransient<IContentSerializer, MessagePackContentSerializer>();
        }

        #endregion

        #region registration

        protected override void ApiRegistration(IRpcApi api)
        {
            api.Expose<IntMath>();
        }

        #endregion
    }
}
