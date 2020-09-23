using System;
using System.Collections.Generic;
using System.Text;

namespace EasyRpc.Abstractions.Path
{
    /// <summary>
    /// Ignore a method export
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class IgnoreMethodAttribute : Attribute
    {

    }
}
