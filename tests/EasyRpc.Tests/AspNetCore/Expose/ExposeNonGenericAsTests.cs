﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using EasyRpc.Abstractions.Path;
using EasyRpc.AspNetCore;
using Xunit;

namespace EasyRpc.Tests.AspNetCore.Expose
{
    public class ExposeNonGenericAsTests : BaseRequestTest
    {
        #region Service

        public class Service
        {
            [PostMethod]
            public int Add(int x, int y)
            {
                return x + y;
            }
        }

        #endregion

        #region Tests

        [Fact]
        public async Task Expose_Non_Generic_AsTest()
        {
            var x = 5;
            var y = 10;

            var response = await Post("/IntMath/Add", new { x, y });

            var result = await Deserialize<int>(response);

            Assert.Equal(x + y, result);
        }

        #endregion

        #region registration

        protected override void ApiRegistration(IRpcApi api)
        {
            api.Expose(typeof(Service)).As("IntMath");
        }

        #endregion
    }
}
