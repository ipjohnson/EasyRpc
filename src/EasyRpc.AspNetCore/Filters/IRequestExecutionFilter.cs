using System;
using System.Collections.Generic;
using System.Text;

namespace EasyRpc.AspNetCore.Filters
{
    public interface IRequestExecutionFilter : IRequestFilter
    {
        /// <summary>
        /// Called before the execution happens. 
        /// </summary>
        /// <param name="context"></param>
        void BeforeExecute(RequestExecutionContext context);

        /// <summary>
        /// Called after the execution
        /// </summary>
        /// <param name="context"></param>
        void AfterExecute(RequestExecutionContext context);
    }
}
