using System;
using System.Collections.Generic;
using System.Text;
using EasyRpc.AspNetCore.Configuration.DelegateConfiguration;

namespace EasyRpc.AspNetCore.Configuration
{
    public class RequestExecutionContextMethods : IRequestExecutionContextMethods
    {
        private readonly IInternalApiConfiguration _internalApiConfiguration;

        public RequestExecutionContextMethods(IInternalApiConfiguration internalApiConfiguration)
        {
            _internalApiConfiguration = internalApiConfiguration;
        }

        /// <inheritdoc />
        public IExposureDelegateConfiguration Get<TResult>(string path, Func<RequestExecutionContext, TResult> handler)
        {
            var configuration = new DelegateInstanceConfiguration(_internalApiConfiguration.GetCurrentApiInformation(), handler);

            _internalApiConfiguration.ApplicationConfigurationService.AddConfigurationObject(configuration);

            return configuration;
        }
    }
}
