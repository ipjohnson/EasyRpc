using System;
using System.Collections.Generic;
using System.Text;

namespace EasyRpc.AspNetCore.Configuration
{
    public interface IInternalApiConfiguration : IApiConfiguration, IEndPointHandlerConfigurationProvider
    {
        ICurrentApiInformation GetCurrentApiInformation();
    }
}
