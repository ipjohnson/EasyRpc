using System;
using System.Reflection;

namespace EasyRpc.AspNetCore
{
    public interface IExposureConfiguration
    {
        IExposureConfiguration As(string name);

        IExposureConfiguration Authorize(string role = null, string policy = null);

        IExposureConfiguration Methods(Func<MethodInfo, bool> methods);
    }

    public interface IExposureConfiguration<T>
    {
        IExposureConfiguration<T> As(string name);

        IExposureConfiguration<T> Authorize(string role = null, string policy = null);

        IExposureConfiguration<T> Methods(Func<MethodInfo, bool> methods);
    }
}
