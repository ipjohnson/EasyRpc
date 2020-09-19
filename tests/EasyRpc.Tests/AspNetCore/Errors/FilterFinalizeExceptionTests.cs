using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using EasyRpc.Abstractions.Path;
using EasyRpc.AspNetCore;
using EasyRpc.AspNetCore.Errors;
using EasyRpc.AspNetCore.Filters;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace EasyRpc.Tests.AspNetCore.Errors
{
    public class FilterFinalizeExceptionTests : BaseRequestTest
    {
        private const string _finalizeError = "Finalize error";
        private string _returnedError;
        private const int _intReturn = 10;

        #region Service

        public class Service
        {
            [GetMethod]
            [InstanceFilter(typeof(FinalizeExceptionFilter))]
            public int GetValue()
            {
                return _intReturn;
            }
        }

        public class FinalizeExceptionFilter : IRequestFinalizeFilter
        {
            public void Finalize(RequestExecutionContext context)
            {
                throw new Exception(_finalizeError);
            }
        }

        #endregion

        #region Tests

        [Fact]
        public async Task Errors_FilterFinalizeException()
        {
            var response = await Get("/Service/GetValue");

            var result = await Deserialize<int>(response);

            Assert.True(result == _intReturn);
            Assert.Equal(_returnedError, _finalizeError);
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
