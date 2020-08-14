using System.Collections.Generic;
using System.Threading.Tasks;

namespace EasyRpc.AspNetCore.Authorization
{
    /// <summary>
    /// Default authorization service
    /// </summary>
    public class EndPointAuthorizationService : IEndPointAuthorizationService
    {
        /// <inheritdoc />
        public async Task<bool> AuthorizeRequest(RequestExecutionContext context,
            IReadOnlyList<IEndPointMethodAuthorization> configurationAuthorizations)
        {
            foreach (var authorization in configurationAuthorizations)
            {
                if (!await authorization.AsyncAuthorize(context))
                {
                    return false;
                }
            }

            return true;
        }
    }
}
