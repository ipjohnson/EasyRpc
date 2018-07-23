using System.Collections.Generic;

namespace EasyRpc.AspNetCore.Middleware
{
    public interface IExposedMethodInformationProvider
    {
        IEnumerable<IExposedMethodInformation> GetExposedMethods();
    }
}
