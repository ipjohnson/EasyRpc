using System;
using System.Collections.Generic;
using System.Net;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using EasyRpc.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Xunit;

namespace EasyRpc.Tests.AspNetCore.Authorization
{
    public class RootLevelAuthorized : BaseRequestTest
    {
        private bool _authorized = false;
        
        #region Tests

        [Fact]
        public async Task Authorization_RootLevelAuthorization()
        {
            var response = await Get("/test");

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);

            _authorized = true;

            response = await Get("/test");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        #endregion

        #region registration

        protected override void ApiRegistration(IRpcApi api)
        {
            api.Authorize();
            api.GetMethod("/test", () => "success");
        }

        protected override void ConfigureAspNetPipeline(IApplicationBuilder app)
        {
            app.Use((context, next) =>
            {
                if (_authorized)
                {
                    ClaimsIdentity claimsIdentity = new ClaimsIdentity(new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, "testuser"),
                        new Claim(ClaimTypes.NameIdentifier, "testuser"),
                    }, "Password");
                    ClaimsPrincipal claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
                    context.User = claimsPrincipal;
                }

                return next();
            });

            base.ConfigureAspNetPipeline(app);
        }

        #endregion
    }
}
