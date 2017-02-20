using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EasyRpc.AspNetCore.Messages;
using Microsoft.AspNetCore.Http;

namespace EasyRpc.AspNetCore
{
    public interface ICallExecuteFilter
    {
        void BeforeExecute(ICallExecutionContext context);

        void AfterExecute(ICallExecutionContext context);
    }
}
