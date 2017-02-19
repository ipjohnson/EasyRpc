using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EasyRpc.AspNetCore.Middleware
{
    public interface IExposedMethodInformationProvider
    {
        IEnumerable<ExposedMethodInformation> GetExposedMethods();
    }
}
