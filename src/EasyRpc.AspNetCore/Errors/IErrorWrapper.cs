using System;
using System.Collections.Generic;
using System.Text;

namespace EasyRpc.AspNetCore.Errors
{
    /// <summary>
    /// Error wrapping interface
    /// </summary>
    public interface IErrorWrapper
    {
        /// <summary>
        /// Error message
        /// </summary>
        string Message { get; set; }
    }
}
