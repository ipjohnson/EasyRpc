using System;
using System.Collections.Generic;
using System.Net;
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
    public class FilterExceptionTests : BaseRequestTest
    {
        private const string _beforeExecuteError = "Before Execute";
        private const string _afterExecuteError = "After Execute";
        private string _returnedError;

        #region Service

        public class Service
        {
            [GetMethod]
            [InstanceFilter(typeof(BeforeExecuteExceptionFilter))]
            public int BeforeError()
            {
                throw new Exception("Execute Exception");
            }

            [GetMethod]
            [InstanceFilter(typeof(AfterExecuteExceptionFilter))]
            public int AfterError()
            {
                return 0;
            }
        }

        public class BeforeExecuteExceptionFilter : IRequestExecutionFilter
        {
            public void BeforeExecute(RequestExecutionContext context)
            {
                throw new Exception(_beforeExecuteError);
            }

            public void AfterExecute(RequestExecutionContext context)
            {
                throw new NotImplementedException("Should not get here");
            }
        }

        public class AfterExecuteExceptionFilter : IRequestExecutionFilter
        {
            public void BeforeExecute(RequestExecutionContext context)
            {
                
            }

            public void AfterExecute(RequestExecutionContext context)
            {
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

        protected override void ApiRegistration(IRpcApi api)
        {
            api.Expose<Service>();
        }

        #endregion
    }
}
