using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using EasyRpc.Abstractions.Path;
using EasyRpc.AspNetCore;
using Xunit;

namespace EasyRpc.Tests.AspNetCore.ModelBinding.AspNetRouting
{
    public class AspNetGetGuidRouteTest : BaseRequestTest
    {
        #region Service

        public class Service
        {
            [GetMethod("/Service/Get/{guid}")]
            public Guid Get(Guid guid)
            {
                return guid;
            }
        }

        #endregion

        #region Tests

        [Fact]
        public async Task ModelBinding_AspNetRouting_GuidParameter()
        {
            var guidString = "E8230402-67BA-41DF-B23C-6B9C1580C249";

            var response = await Get($"/Service/Get/{guidString}");

            var result = await Deserialize<Guid>(response);
        }

        #endregion

        #region registration

        protected override bool UseInternalRouting => false;

        protected override void ApiRegistration(IRpcApi api)
        {
            api.Expose<Service>();
        }

        #endregion
    }
}
