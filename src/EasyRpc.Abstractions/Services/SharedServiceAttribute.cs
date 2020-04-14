using System;
using System.Collections.Generic;
using System.Text;

namespace EasyRpc.Abstractions.Services
{
    public class SharedServiceAttribute : Attribute, IServiceActivationAttribute
    {
        public bool FromServiceContainer { get; set; } = false;
        
        public ServiceActivationMethod ActivationMethod => FromServiceContainer
            ? ServiceActivationMethod.SharedServiceContainer
            : ServiceActivationMethod.SharedActivationUtility;
    }
}
