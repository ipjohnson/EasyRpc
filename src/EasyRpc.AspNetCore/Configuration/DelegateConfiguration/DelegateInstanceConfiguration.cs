using System;
using System.Collections.Generic;
using System.Text;

namespace EasyRpc.AspNetCore.Configuration.DelegateConfiguration
{
    public class DelegateInstanceConfiguration : BaseDelegateInstanceConfiguration
    {
        private readonly ICurrentApiInformation _currentApi;
        private readonly Delegate _delegate;

        public DelegateInstanceConfiguration(ICurrentApiInformation currentApi, Delegate @delegate)
        {
            _currentApi = currentApi;
            _delegate = @delegate;
        }

        public override void ExecuteConfiguration(IApplicationConfigurationService service)
        {
            service.ExposeDelegate(_currentApi,this, _delegate);
        }
    }
}
