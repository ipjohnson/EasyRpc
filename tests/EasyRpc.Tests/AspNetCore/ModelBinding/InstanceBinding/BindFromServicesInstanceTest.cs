using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using EasyRpc.Abstractions.Binding;
using EasyRpc.Abstractions.Path;
using EasyRpc.AspNetCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace EasyRpc.Tests.AspNetCore.ModelBinding.InstanceBinding
{
    public class BindFromServicesInstanceTest : BaseRequestTest
    {
        #region Service

        public class Service
        {
            [BindFromServices]
            public IDataSource DataSource { get; set; }

            [GetMethod]
            public int Get()
            {
                return DataSource.Value;
            }
        }

        public interface IDataSource
        {
            int Value { get; }
        }

        public class DataSource : IDataSource
        {
            public int Value => 10;
        }

        #endregion

        #region Tests

        [Fact]
        public async Task ModelBinding_InstanceBinding_BindFromServices()
        {
            var response = await Get("/Service/Get");

            var result = await Deserialize<int>(response);

            Assert.Equal(10, result);
        }

        #endregion

        #region Registration

        /// <inheritdoc />
        protected override void ConfigureServices(IServiceCollection services)
        {
            base.ConfigureServices(services);
            services.AddTransient<IDataSource, DataSource>();
        }

        protected override void ApiRegistration(IRpcApi api)
        {
            api.Expose<Service>();
        }

        #endregion
    }
}
