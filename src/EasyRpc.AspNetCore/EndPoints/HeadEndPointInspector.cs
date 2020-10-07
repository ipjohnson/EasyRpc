using System;
using System.Collections.Generic;
using System.Text;
using EasyRpc.AspNetCore.Routing;
using Microsoft.AspNetCore.Http;

namespace EasyRpc.AspNetCore.EndPoints
{
    public class HeadEndPointInspector : IApiEndPointInspector
    {
        public IEnumerable<IEndPointMethodHandler> InspectEndPoints(Dictionary<string, Dictionary<string, IEndPointMethodHandler>> endPoints)
        {
            foreach (var endPointPair in endPoints)
            {
                if (endPointPair.Value.TryGetValue(HttpMethods.Get, out var getHandler))
                {
                    var configuration = new EndPointMethodConfiguration(
                        new RpcRouteInformation
                        {
                            HasBody = false,
                            Method = HttpMethods.Head,
                            RouteBasePath = getHandler.RouteInformation.RouteBasePath,
                            RouteTemplate = getHandler.RouteInformation.RouteTemplate,
                            Tokens = getHandler.RouteInformation.Tokens
                        }, 
                        context => null, 
                        getHandler.Configuration.InvokeInformation, 
                        typeof(void));

                    configuration.Parameters.AddRange(getHandler.Configuration.Parameters);

                    yield return new HeadEndPointMethodHandler(getHandler, configuration);
                }
            }
        }
    }
}
