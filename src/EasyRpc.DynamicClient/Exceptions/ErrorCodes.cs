using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EasyRpc.DynamicClient.Exceptions
{

    /// <summary>
    /// error codes from http://www.jsonrpc.org/specification#error_object
    /// </summary>
    public enum JsonRpcErrorCode
    {
        /// <summary>
        /// Parse error
        /// </summary>
        ParseError = -32700,

        /// <summary>
        /// Invalid request
        /// </summary>
        InvalidRequest = -32600,

        /// <summary>
        /// Method Not Found
        /// </summary>
        MethodNotFound = -32601,

        /// <summary>
        /// Internal server error
        /// </summary>
        InternalServerError = -32603,

        /// <summary>
        /// Unauthorized access
        /// </summary>
        UnauthorizedAccess = -32000
    }
}
