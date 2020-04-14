using System;
using System.Collections.Generic;
using System.Text;

namespace EasyRpc.Abstractions.Services
{
    public enum ServiceActivationMethod
    {
        /// <summary>
        /// ActivatorUtilities.GetServiceOrCreateInstance will be used to activate services
        /// </summary>
        ActivationUtility,

        /// <summary>
        /// Locate service instance from
        /// </summary>
        ServiceContainer,

        /// <summary>
        /// Shared instance of service activated using ActivatorUtilities.GetServiceOrCreateInstance
        /// </summary>
        SharedActivationUtility,

        /// <summary>
        /// Shared instance of service activated using the services container
        /// </summary>
        SharedServiceContainer
    }

    public interface IServiceActivationAttribute
    {
        ServiceActivationMethod ActivationMethod { get; }
    }
}
