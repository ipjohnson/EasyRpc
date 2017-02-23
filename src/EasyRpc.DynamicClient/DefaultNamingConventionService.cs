using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace EasyRpc.DynamicClient
{
    public class DefaultNamingConventionService : INamingConventionService
    {
        public string GetNameForType(Type type)
        {
            return type.Name;
        }

        public string GetMethodName(MethodInfo method)
        {
            return method.Name;
        }
    }
}

