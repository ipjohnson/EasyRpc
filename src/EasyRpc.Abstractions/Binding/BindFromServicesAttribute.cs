using System;
using System.Collections.Generic;
using System.Text;

namespace EasyRpc.Abstractions.Binding
{
    /// <summary>
    /// Bind from services
    /// </summary>

    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter)]
    public class BindFromServicesAttribute : Attribute
    {

    }
}
