using System.Collections.Generic;
using System.Threading.Tasks;
using EasyRpc.AspNetCore.Messages;
using Microsoft.AspNetCore.Http;

namespace EasyRpc.AspNetCore
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="instance"></param>
    /// <param name="parameterValues"></param>
    /// <returns></returns>
    public delegate Task<ResponseMessage> InvokeMethodWithArray(object instance, object[] parameterValues, string version, string id);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="values"></param>
    /// <returns></returns>
    public delegate object[] OrderedParametersToArray(object[] values, HttpContext context);
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="values"></param>
    /// <returns></returns>
    public delegate object[] NamedParametersToArray(IDictionary<string, object> values, HttpContext context);

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
