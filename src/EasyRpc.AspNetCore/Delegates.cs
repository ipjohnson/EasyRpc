using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EasyRpc.AspNetCore.Messages;
using Microsoft.AspNetCore.Http;

namespace EasyRpc.AspNetCore
{
    /// <summary>
    /// Delegate to invoke method on instance, named parameters
    /// </summary>
    /// <param name="instance"></param>
    /// <param name="values"></param>
    /// <returns></returns>
    public delegate Task<ResponseMessage> InvokeMethodByNamedParameters(string version, string id, object instance, IDictionary<string, object> values, HttpContext httpContext);

    /// <summary>
    /// Delegate to invoke method on instance, parameters in order
    /// </summary>
    /// <param name="version"></param>
    /// <param name="id"></param>
    /// <param name="instance"></param>
    /// <param name="values"></param>
    /// <returns></returns>
    public delegate Task<ResponseMessage> InvokeMethodOrderedParameters(string version, string id, object instance, object[] values, HttpContext httpContext);
}
