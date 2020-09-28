using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using EasyRpc.AspNetCore;
using Microsoft.AspNetCore.Authorization;
using Xunit;

namespace EasyRpc.Tests.AspNetCore.Authorization
{
    public class ExposeLevelAuthorizeAttributeTest : BaseRequestTest
    {
        #region Tests

        [Fact]
        public async Task Authorization_ExposeLevelAuthorizeAttributeTest()
        {

        }

        #endregion

        #region registration

        protected override void ApiRegistration(IRpcApi api)
        {

        }

        #endregion

        #region Service

        public class Service
        {
            [Authorize]
            public string SomeMethod()
            {
                return "SomeString";
            }
        }

        #endregion

    }
}
