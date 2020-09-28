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
    public class InstantiateExceptionTest : BaseRequestTest
    {
        private const string _errorMessage = "Some error";
        private string _returnedError;

        #region Service

        public class Service
        {
            public Service()
            {
                throw new Exception(_errorMessage);
            }

            [GetMethod]
            public int Error()
            {
                return 0;
            }

            [GetMethod("/Service/ErrorValue/{value}")]
            public int ErrorValue(int value)
            {
                return value;
            }
        }

        #endregion

        #region Tests

        [Fact]
        public async Task Errors_InstantiateException()
        {
            var response = await Get("/Service/Error");

            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
            Assert.Equal(_errorMessage, _returnedError);
        }

        [Fact]
        public async Task Errors_InstantiateException_WithParameters()
        {
            var response = await Get("/Service/ErrorValue/10");

            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
            Assert.Equal(_errorMessage, _returnedError);
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

        protected override void ApiRegistration(IRpcApi api)
        {
            api.Expose<Service>();
        }
        #endregion
    }
}
