using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using EasyRpc.Abstractions.Path;
using EasyRpc.AspNetCore;
using EasyRpc.AspNetCore.Features;
using EasyRpc.Tests.AspNetCore.Expose;
using Microsoft.AspNetCore.Http;
using Xunit;

namespace EasyRpc.Tests.AspNetCore.Features
{
    public class RequestExecutionContextFeatureTests : BaseRequestTest
    {
        #region Service

        public class Service
        {
            [GetMethod]
            public bool Get(HttpContext context)
            {
                var feature = context.Features.Get<IRequestExecutionContextFeature>();

                return feature?.Context != null;
            }
        }

        #endregion

        #region Tests

        [Fact]
        public async Task Features_RequestExecutionContextFeature()
        {
            var response = await Get("/Service/Get");

            var result = await Deserialize<bool>(response);

            Assert.True(result);
        }

        #endregion

        #region registration

        protected override void ApiRegistration(IApiConfiguration api)
        {
            api.UseRequestExecutionContextFeature();
            api.Expose<Service>();
        }

        #endregion
    }
}
