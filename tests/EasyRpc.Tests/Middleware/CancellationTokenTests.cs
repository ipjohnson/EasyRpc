using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using EasyRpc.AspNetCore.Messages;
using EasyRpc.Tests.Classes;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using SimpleFixture.xUnit;
using Xunit;

namespace EasyRpc.Tests.Middleware
{
    public class CancellationTokenTests : BaseRpcMiddlewareTests
    {

        [Theory]
        [AutoData]
        public async Task CancellationToken(IApplicationBuilder app, HttpContext context)
        {
            Configure(app, "RpcApi", api =>
            {
                api.Expose<CancellationTokenService>().As("CancellationToken");
            });

            var cancellationSource = new CancellationTokenSource();

            cancellationSource.CancelAfter(500);
            context.RequestAborted = cancellationSource.Token;

            var result = await MakeCallAsync<ErrorResponseMessage>(context, "/RpcApi/CancellationToken", "Wait", new[] { 2000 });
            
            Assert.NotNull(result);
            Assert.Contains("A task was canceled", result.Error.Message);
        }
    }
}
