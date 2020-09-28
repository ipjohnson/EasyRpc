using System;
using System.Collections.Generic;
using System.Text;

namespace EasyRpc.AspNetCore.Configuration
{
    public interface IInternalApiConfiguration : IRpcApi, IEndPointHandlerConfigurationProvider
    {
        IApplicationConfigurationService ApplicationConfigurationService { get; }

        ICurrentApiInformation GetCurrentApiInformation();
    }
}
