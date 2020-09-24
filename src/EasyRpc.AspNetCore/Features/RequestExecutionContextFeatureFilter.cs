using System;
using System.Collections.Generic;
using System.Text;
using EasyRpc.AspNetCore.Filters;

namespace EasyRpc.AspNetCore.Features
{
    public class RequestExecutionContextFeatureFilter : IRequestExecutionFilter
    {
        /// <inheritdoc />
        public void BeforeExecute(RequestExecutionContext context)
        {
            context.HttpContext.Features.Set<IRequestExecutionContextFeature>(new RequestExecutionContextFeature(context));
        }

        /// <inheritdoc />
        public void AfterExecute(RequestExecutionContext context)
        {
            
        }
    }
}
