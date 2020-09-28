using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using EasyRpc.Abstractions.Path;
using EasyRpc.AspNetCore;
using Xunit;

namespace EasyRpc.Tests.AspNetCore.ModelBinding.Special
{
    public class CancellationTokenTest : BaseRequestTest
    {
        #region Service

        public class Service
        {
            [GetMethod("/testinvoke/{i}")]
            public Task<int> TestInvoke(int i, CancellationToken token)
            {
                return Task.FromResult(i);
            }
        }

        #endregion
        
        #region Tests

        [Fact]
        public async Task CancellationTokenTest_AsParameter()
        {
            var arg1 = 10;

            var response = await Get($"/testinvoke/{arg1}");

            var value = await Deserialize<int>(response);

            Assert.Equal(arg1, value);
        }

        #endregion

        #region registration

        protected override void ApiRegistration(IRpcApi api)
        {
            api.Expose<Service>();
        }

        #endregion
    }
}
