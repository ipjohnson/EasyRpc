using System;
using System.Reflection;

namespace EasyRpc.DynamicClient
{
    public class DefaultNamingConventionService : INamingConventionService
    {
        public virtual string GetNameForType(Type type)
        {
            return type.Name;
        }

        public virtual string GetMethodName(MethodInfo method)
        {
            return method.Name;
        }
    }
}

