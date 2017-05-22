using System;
using System.Reflection;

namespace EasyRpc.DynamicClient
{
    public interface INamingConventionService
    {
        string GetNameForType(Type type);

        string GetMethodName(MethodInfo method);
    }
}
