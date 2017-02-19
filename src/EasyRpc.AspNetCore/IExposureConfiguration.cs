using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EasyRpc.AspNetCore
{
    public interface IExposureConfiguration
    {
        IExposureConfiguration As(string name);
    }

    public interface IExposureConfiguration<T>
    {
        IExposureConfiguration<T> As(string name);
    }
}
