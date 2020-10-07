using System;
using System.Collections.Generic;
using System.Text;
using EasyRpc.AspNetCore.EndPoints;

namespace EasyRpc.AspNetCore
{
    public interface IApiEndPointInspector
    {
        IEnumerable<IEndPointMethodHandler> InspectEndPoints(
            Dictionary<string, Dictionary<string, IEndPointMethodHandler>> endPoints);
    }
}
