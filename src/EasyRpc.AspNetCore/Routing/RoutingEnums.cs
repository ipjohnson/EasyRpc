using System;
using System.Collections.Generic;
using System.Text;

namespace EasyRpc.AspNetCore.Routing
{
    /// <summary>
    /// Represents what type of token either parameter or just a match
    /// </summary>
    public enum RpcRouteTokenType
    {
        /// <summary>
        /// Is parameter that needs to be bound
        /// </summary>
        Parameter,

        /// <summary>
        /// Token must match a portion of the path but is not a parameter to the method
        /// </summary>
        MatchPath,
    }

    /// <summary>
    /// Represents how the token should be parsed
    /// </summary>
    public enum RpcRouteTokenParseType
    {
        Integer,

        Decimal,

        Double,

        DateTime,

        String,

        GUID
    }
}
