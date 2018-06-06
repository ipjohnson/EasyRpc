using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace EasyRpc.AspNetCore.Middleware
{
    public interface IInstanceActivator
    {
        object ActivateInstance(HttpContext context, IServiceProvider serviceProvider, Type instanceType);
    }

    public class InstanceActivator : IInstanceActivator
    {
        public object ActivateInstance(HttpContext context, IServiceProvider serviceProvider, Type instanceType)
        {
            return ActivatorUtilities.GetServiceOrCreateInstance(serviceProvider, instanceType);
        }
    }
}
