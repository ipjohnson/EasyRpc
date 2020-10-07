using System;
using System.Collections.Generic;
using System.Text;
using EasyRpc.AspNetCore.EndPoints;

namespace EasyRpc.AspNetCore.Configuration
{
    public interface IEndPointHandlerConfigurationProvider
    {
        Dictionary<string, Dictionary<string, IEndPointMethodHandler>> GetEndPointHandlers();
    }
}
