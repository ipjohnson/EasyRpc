using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using EasyRpc.AspNetCore;
using EasyRpc.AspNetCore.Filters;
using Xunit;

namespace EasyRpc.Tests.AspNetCore.Filters
{
    public class FinalizeFilterTest : BaseRequestTest
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

        protected override void ApiRegistration(IRpcApi api)
        {
            var filter = new FinalizeFilter(() => _finalizedCalled = true);

            api.ApplyFilter(method => context => filter);
            api.Method.Get("/test", () => new {result = 123});
        }

        #endregion

        #region FinalizeFilter

        public class FinalizeFilter : IRequestFinalizeFilter
        {
            private readonly Action _finalizeAction;

            public FinalizeFilter(Action finalizeAction)
            {
                _finalizeAction = finalizeAction;
            }

            /// <inheritdoc />
            public void Finalize(RequestExecutionContext context)
            {
                _finalizeAction();
            }
        }
        #endregion
    }
}
