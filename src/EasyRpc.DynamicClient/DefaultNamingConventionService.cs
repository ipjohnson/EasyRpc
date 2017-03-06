using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

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

