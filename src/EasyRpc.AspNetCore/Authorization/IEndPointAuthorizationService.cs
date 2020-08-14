using System.Collections.Generic;
using System.Threading.Tasks;

namespace EasyRpc.AspNetCore.Authorization
{
    /// <summary>
    /// authorization service
    /// </summary>
    public interface IEndPointAuthorizationService
    {
        /// <summary>
        /// Authorize a request given a context and list of authorizations
        /// </summary>
        /// <param name="context">request execution context</param>
        /// <param name="configurationAuthorizations">list of authorizations</param>
        /// <returns></returns>
        Task<bool> AuthorizeRequest(RequestExecutionContext context,
            IReadOnlyList<IEndPointMethodAuthorization> configurationAuthorizations);
    }
}
