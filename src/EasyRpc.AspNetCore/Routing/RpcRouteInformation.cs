using System;
using System.Collections.Generic;
using System.Text;

namespace EasyRpc.AspNetCore.Routing
{
    public interface IRpcRouteInformation
    {
        string Method { get; }

        bool HasBody { get; }

        string RouteTemplate { get; }

        string RouteBasePath { get; }

        IReadOnlyList<IRpcRouteToken> Tokens { get; }
    }

    public class RpcRouteInformation : IRpcRouteInformation
    {
        public string Method { get; set; }

        public bool HasBody { get; set; }

        public string RouteTemplate { get; set; }

        public string RouteBasePath { get; set; }

        public IReadOnlyList<IRpcRouteToken> Tokens { get; set; }
    }
}
