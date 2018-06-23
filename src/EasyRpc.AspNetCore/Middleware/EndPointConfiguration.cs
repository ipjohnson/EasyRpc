using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace EasyRpc.AspNetCore.Middleware
{
    public class EndPointConfiguration
    {
        public EndPointConfiguration(string route, 
            ConcurrentDictionary<string, ExposedMethodInformation> methods,
            bool enableDocumentation, 
            DocumentationConfiguration documentationConfiguration)
        {
            Route = route;
            Methods = methods;
            EnableDocumentation = enableDocumentation;
            DocumentationConfiguration = documentationConfiguration;
        }

        public string Route { get; }

        public ConcurrentDictionary<string, ExposedMethodInformation> Methods { get; }

        public bool EnableDocumentation { get; }

        public DocumentationConfiguration DocumentationConfiguration { get; }
    }
}
