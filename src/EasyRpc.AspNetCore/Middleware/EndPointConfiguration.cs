using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace EasyRpc.AspNetCore.Middleware
{
    public class EndPointConfiguration
    {
        public EndPointConfiguration(string route, ConcurrentDictionary<string, ExposedMethodInformation> methods)
        {
            Route = route;
            Methods = methods;
        }

        public string Route { get; }

        public ConcurrentDictionary<string, ExposedMethodInformation> Methods { get; }
    }
}
