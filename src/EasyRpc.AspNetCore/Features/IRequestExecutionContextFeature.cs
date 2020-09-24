using System;
using System.Collections.Generic;
using System.Text;

namespace EasyRpc.AspNetCore.Features
{
    public interface IRequestExecutionContextFeature
    {
        RequestExecutionContext Context { get; }
    }
}
