using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EasyRpc.AspNetCore.Messages;
using Microsoft.AspNetCore.Http;

namespace EasyRpc.AspNetCore
{
    /// <summary>
    /// Execute filter
    /// </summary>
    public interface ICallExecuteFilter : ICallFilter
    {
        void BeforeExecute(ICallExecutionContext context);

        void AfterExecute(ICallExecutionContext context);
    }
}
