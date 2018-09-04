using System.Threading.Tasks;
using EasyRpc.AspNetCore.Messages;

namespace EasyRpc.AspNetCore
{
    /// <summary>
    /// Delegate for invoking a method given an array of parameters
    /// </summary>
    /// <param name="instance">instance of the object to invoke on</param>
    /// <param name="parameterValues">parameters for the method to invoke</param>
    /// <param name="version">version for message</param>
    /// <param name="id">message id</param>
    /// <returns>rpc response message</returns>
    public delegate Task<ResponseMessage> InvokeMethodWithArray(object instance, object[] parameterValues, string version, string id);

}
