using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using EasyRpc.AspNetCore;
using Xunit;

namespace EasyRpc.Tests.AspNetCore.Serialization.SystemTextJson
{
    public class BrotliResponseEncoding : BaseRequestTest
    {
        #region Tests

        //[Fact]
        public async Task Serialization_SystemTextJson_BrotliResponseEncoding()
        {

        }

        #endregion

        #region Registration

        protected override void ApiRegistration(IApiConfiguration api)
        {
            
        }

        #endregion
    }
}
