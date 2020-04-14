using System;
using System.Collections.Generic;
using System.Text;
using EasyRpc.AspNetCore.EndPoints;

namespace EasyRpc.AspNetCore.Configuration
{
    public interface IApiConfigurationFactory
    {
        IInternalApiConfiguration CreateApiConfiguration(IServiceProvider serviceProvider);
    }

    public class ApiConfigurationFactory : IApiConfigurationFactory
    {
        public IInternalApiConfiguration CreateApiConfiguration(IServiceProvider serviceProvider)
        {
            return new InternalApiConfiguration(serviceProvider);
        }
    }
}
