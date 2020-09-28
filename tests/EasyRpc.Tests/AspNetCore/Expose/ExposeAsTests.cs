using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using EasyRpc.AspNetCore;
using Xunit;

namespace EasyRpc.Tests.AspNetCore.Expose
{
    public class ExposeAsTests : BaseRequestTest
    {
        #region Service

        public class Service
        {
            public int Add(int x, int y)
            {
                return x + y;
            }
        }

        #endregion

        #region Tests

        [Fact]
        public async Task Expose_AsTest()
        {
            var x = 5;
            var y = 10;

            var response = await Post("/IntMath/Add", new {x, y});

            var result = await Deserialize<int>(response);

            Assert.Equal(x + y, result);
        }

        #endregion

        #region registration

        protected override void ApiRegistration(IRpcApi api)
        {
            api.Expose<Service>().As("IntMath");
        }

        #endregion
    }
}
