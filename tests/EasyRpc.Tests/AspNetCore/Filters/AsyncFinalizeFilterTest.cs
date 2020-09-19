using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EasyRpc.AspNetCore;
using EasyRpc.AspNetCore.Filters;
using Xunit;

namespace EasyRpc.Tests.AspNetCore.Filters
{
    public class AsyncFinalizeFilterTest : BaseRequestTest
    {
        private bool _finalizedCalled;

        #region Tests

        [Fact]
        public async Task Filters_FinalizeFilter()
        {
            var result = await Get("/test");

            Assert.True(_finalizedCalled);
        }

        #endregion

        #region Registration

        protected override void ApiRegistration(IApiConfiguration api)
        {
            var filter = new FinalizeFilterTest.FinalizeFilter(() => _finalizedCalled = true);

            api.ApplyFilter(method => context => filter);
            api.GetMethod("/test", () => new { result = 123 });
        }

        #endregion

        #region FinalizeFilter

        public class FinalizeFilter : IAsyncRequestFinalizeFilter
        {
            private readonly Action _finalizeAction;

            public FinalizeFilter(Action finalizeAction)
            {
                _finalizeAction = finalizeAction;
            }

            public async Task FinalizeAsync(RequestExecutionContext context)
            {
                await Task.Delay(1);

                _finalizeAction();
            }
        }
        #endregion
    }
}
