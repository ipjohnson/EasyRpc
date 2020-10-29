using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using EasyRpc.Abstractions.Path;
using EasyRpc.AspNetCore;
using EasyRpc.AspNetCore.Errors;
using EasyRpc.Tests.AspNetCore.Errors;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace EasyRpc.Tests.AspNetCore.ModelBinding.InternalRouting
{
    public class DoubleParameterTests : BaseRequestTest
    {
        private string _returnedError;

        #region Service

        public class Service
        {
            [GetMethod]
            public double Value(double value)
            {
                return value;
            }

            [GetMethod]
            public double Value2(double value = 123.456)
            {
                return value;
            }
        }

        #endregion

        #region Tests

        [Fact]
        public async Task ModelBinding_InternalRouting_DoubleParameter()
        {
            var value = 123.456;

            var response = await Get($"/Service/Value/{value}");

            var result = await Deserialize<double>(response);

            Assert.Equal(value, result);
        }

        [Fact]
        public async Task ModelBinding_InternalRouting_DoubleParameter_Invalid()
        {
            var response = await Get($"/Service/Value2/blah-blah");

            var result = await Deserialize<double>(response);

            Assert.Equal(123.456, result);
        }

        #endregion

        #region Registration

        protected override bool UseInternalRouting => true;

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
