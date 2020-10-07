using System;
using System.Collections.Generic;
using System.Text;
using EasyRpc.AspNetCore.EndPoints;

namespace EasyRpc.AspNetCore.Documentation
{
    public class DocumentationEndPointInspector : IApiEndPointInspector
    {
        public IEnumerable<IEndPointMethodHandler> InspectEndPoints(Dictionary<string, Dictionary<string, IEndPointMethodHandler>> endPoints)
        {


            yield break;
        }
    }
}
