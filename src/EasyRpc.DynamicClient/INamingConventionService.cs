using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace EasyRpc.DynamicClient
{
    public interface INamingConventionService
    {
        string GetNameForType(Type type);

        string GetMethodName(MethodInfo method);
    }
}
