using System;
using System.Collections.Generic;
using System.Text;

namespace EasyRpc.AspNetCore.Features
{
    public interface IRequestExecutionContextFeature
    {
        RequestExecutionContext Context { get; }
    }

    public class RequestExecutionContextFeature : IRequestExecutionContextFeature
    {
        public RequestExecutionContextFeature(RequestExecutionContext context)
        {
            Context = context;
        }

        /// <inheritdoc />
        public RequestExecutionContext Context { get; }
    }
}
