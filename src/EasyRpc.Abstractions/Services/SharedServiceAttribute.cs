using System;
using System.Collections.Generic;
using System.Text;

namespace EasyRpc.Abstractions.Services
{
    /// <summary>
    /// Service should be shared between request instead of creating a new one
    /// </summary>
    public class SharedServiceAttribute : Attribute, IServiceActivationAttribute
    {
        /// <summary>
        /// Resolve service from container
        /// </summary>
        public bool FromServiceContainer { get; set; } = false;
        
        /// <inheritdoc />
        ServiceActivationMethod IServiceActivationAttribute.ActivationMethod => FromServiceContainer
            ? ServiceActivationMethod.SharedServiceContainer
            : ServiceActivationMethod.SharedActivationUtility;
    }
}
