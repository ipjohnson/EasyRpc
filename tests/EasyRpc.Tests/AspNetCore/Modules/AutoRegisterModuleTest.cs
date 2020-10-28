using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using EasyRpc.Abstractions.Path;
using EasyRpc.AspNetCore;
using Xunit;

namespace EasyRpc.Tests.AspNetCore.Modules
{
    public class AutoRegisterModuleTest : BaseRequestTest
    {
        #region Module

        public class IntMathModule : RpcModule
        {
            public IntMathModule()
            {
                this.ToString();
            }
            [PostMethod]
            public int Add(int x, int y)
            {
                return x + y;
            }
        }

        #endregion

        #region Tests

        [Fact]
        public async Task Modules_AutoRegisterModule()
        {
            var response = await Post("/IntMath/Add", new {x = 10, y = 5});

            var result = await Deserialize<int>(response);

            Assert.Equal(15, result);
        }

        #endregion

        #region registration

        protected override void ApiRegistration(IRpcApi api)
        {
            api.ExposeModule<IntMathModule>();
        }

        #endregion
    }
}
