using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using EasyRpc.Abstractions.Path;
using EasyRpc.AspNetCore;
using EasyRpc.AspNetCore.Errors;
using EasyRpc.AspNetCore.Filters;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace EasyRpc.Tests.AspNetCore.Errors
{
    public class FilterAsyncExceptionTests : BaseRequestTest
    {
        private const string _beforeExecuteError = "Before Execute";
        private const string _afterExecuteError = "After Execute";
        private string _returnedError;

        #region Service

        public class Service
        {
            [GetMethod]
            [InstanceFilter(typeof(AsyncBeforeExecuteExceptionFilter))]
            public int BeforeError()
            {
                throw new Exception("Execute Exception");
            }

            [GetMethod]
            [InstanceFilter(typeof(AsyncAfterExecuteExceptionFilter))]
            public int AfterError()
            {
                return 0;
            }
        }

        public class AsyncBeforeExecuteExceptionFilter : IAsyncRequestExecutionFilter
        {
            public async Task BeforeExecuteAsync(RequestExecutionContext context)
            {
                await Task.Delay(1);

                throw new Exception(_beforeExecuteError);
            }

            public Task AfterExecuteAsync(RequestExecutionContext context)
            {
                throw new NotImplementedException();
            }
        }

        public class AsyncAfterExecuteExceptionFilter : IAsyncRequestExecutionFilter
        {
            public Task BeforeExecuteAsync(RequestExecutionContext context)
            {
                return Task.CompletedTask;
            }

            public async Task AfterExecuteAsync(RequestExecutionContext context)
            {
                await Task.Delay(1);

                throw new Exception(_afterExecuteError);
            }
        }

        #endregion

        #region Tests

        [Fact]
        public async Task Errors_FilterException_BeforeExecute()
        {
            var response = await Get("/Service/BeforeError");

            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
            Assert.Equal(_beforeExecuteError, _returnedError);
        }

        [Fact]
        public async Task Errors_FilterException_AfterExecute()
        {
            var response = await Get("/Service/AfterError");

            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
            Assert.Equal(_afterExecuteError, _returnedError);
        }

        #endregion

        #region registration

        /// <inheritdoc />
        protected override void ConfigureServices(IServiceCollection services)
        {
            base.ConfigureServices(services);
            services.AddScoped<IErrorHandler>(provider =>
                new CustomErrorHandler(provider.GetRequiredService<IErrorWrappingService>(), exp =>
                {
                    _returnedError = exp.Message;
                }));
        }

        protected override void ApiRegistration(IApiConfiguration api)
        {
            api.Expose<Service>();
        }

        #endregion
    }
}
