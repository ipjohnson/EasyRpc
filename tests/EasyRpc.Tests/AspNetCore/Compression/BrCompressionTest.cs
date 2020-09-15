using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using EasyRpc.AspNetCore;
using Xunit;

namespace EasyRpc.Tests.AspNetCore.Compression
{
    public class BrCompressionTest : BaseRequestTest
    {
        #region Service

        public class Service
        {

        }

        #endregion

        #region Tests

        [Fact]
        public async Task BrCompressionTest_Test()
        {

        }

        #endregion

        #region registration

        protected override void ApiRegistration(IApiConfiguration api)
        {
            api.Configure.EnableCompression();
        }

        #endregion
    }
}
