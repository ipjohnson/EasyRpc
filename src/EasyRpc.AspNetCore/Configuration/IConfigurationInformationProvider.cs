using System;
using System.Collections.Generic;
using System.Text;

namespace EasyRpc.AspNetCore.Configuration
{
    public interface IConfigurationInformationProvider
    {
        void ExecuteConfiguration(IApplicationConfigurationService service);
    }
}
