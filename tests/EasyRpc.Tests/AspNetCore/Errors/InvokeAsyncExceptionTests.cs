using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using EasyRpc.Abstractions.Path;
using EasyRpc.AspNetCore;
using EasyRpc.AspNetCore.Errors;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace EasyRpc.Tests.AspNetCore.Errors
{
    public class InvokeAsyncExceptionTests : BaseRequestTest
    {
        private const string _errorMessage = "Some error";
        private const string _errorWithParametersMessage = "Some other error";
        private string _returnedError;

        #region Service

        public class Service
        {
            [GetMethod]
            public async Task<int> Error()
            {
                // make sure we go async
                await Task.Delay(1);

                throw new Exception(_errorMessage);
            }
            
            [GetMethod("/Service/ErrorValue/{value}")]
            public async Task<int> ErrorValue(int value)
            {
                await Task.Delay(1);

                throw new Exception(_errorWithParametersMessage);
            }
        }

        #endregion

        #region Tests

        [Fact]
        public async Task Errors_InvokeException()
        {
            var response = await Get("/Service/Error");

            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
            Assert.Equal(_errorMessage, _returnedError);
        }
        
        [Fact]
        public async Task Errors_InvokeException_WithParameters()
        {
            var response = await Get("/Service/ErrorValue/10");

            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
            Assert.Equal(_errorWithParametersMessage, _returnedError);
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
