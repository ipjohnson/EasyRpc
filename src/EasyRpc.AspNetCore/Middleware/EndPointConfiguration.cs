using System.Collections.Generic;

namespace EasyRpc.AspNetCore.Middleware
{
    public class EndPointConfiguration
    {
        public EndPointConfiguration(string route, 
            Dictionary<string, IExposedMethodInformation> methods,
            bool enableDocumentation, 
            DocumentationConfiguration documentationConfiguration)
        {
            Route = route;
            Methods = methods;
            EnableDocumentation = enableDocumentation;
            DocumentationConfiguration = documentationConfiguration;
        }

        public string Route { get; }

        public Dictionary<string, IExposedMethodInformation> Methods { get; }

        public bool EnableDocumentation { get; }

        public DocumentationConfiguration DocumentationConfiguration { get; }
    }
}
