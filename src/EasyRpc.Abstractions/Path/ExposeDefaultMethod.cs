using System;
using System.Collections.Generic;
using System.Text;

namespace EasyRpc.Abstractions.Path
{

    /// <summary>
    /// Http method to use when exposing methods
    /// </summary>
    public enum ExposeDefaultMethod
    {
        /// <summary>
        /// Always use post when no method configured through Attribute or Fluent
        /// </summary>
        PostOnly,

        /// <summary>
        /// Post when there are parameters, Get when no parameters
        /// </summary>
        PostAndGet,

        /// <summary>
        /// Post when more than int parameters, Get when no parameters and int
        /// </summary>
        PostAndGetInt,

        /// <summary>
        /// Get Only methods
        /// </summary>
        GetOnly
    }
}
