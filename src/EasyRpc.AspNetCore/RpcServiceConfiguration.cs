using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EasyRpc.AspNetCore
{
    public class RpcServiceConfiguration
    {
        public bool ShowErrorMessage { get; set; } = true;

        public bool DebugLogging { get; set; } = true;
    }
}
