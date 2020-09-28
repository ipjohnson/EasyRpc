using System;
using System.Collections.Generic;
using System.Text;

namespace EasyRpc.AspNetCore
{
    /// <summary>
    /// Rpc module
    /// </summary>
    public interface IRpcModule
    {
        /// <summary>
        /// Configure module
        /// </summary>
        /// <param name="api"></param>
        void Configure(IRpcApi api);
    }
}
