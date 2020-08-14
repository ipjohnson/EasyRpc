﻿using System;
using System.Collections.Generic;
using System.Text;

namespace EasyRpc.AspNetCore.Routing
{
    public interface IRpcRouteToken
    {
        string Name { get; set; }
        RpcRouteTokenType TokenType { get; set; }
        RpcRouteTokenParseType ParseType { get; set; }
        int ParameterIndex { get; set; }
        bool? Optional { get; set; }
    }

    public class RpcRouteToken : IRpcRouteToken
    {
        public string Name { get; set; }

        public RpcRouteTokenType TokenType { get; set; }

        public RpcRouteTokenParseType ParseType { get; set; }

        public int ParameterIndex { get; set; }

        public bool? Optional { get; set; }
    }
}
