using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EasyRpc.AspNetCore.Middleware
{
    public interface ITypeManager
    {
        Type LookType(string typeName);
    }

    public class TypeManager : ITypeManager
    {
        public Type LookType(string typeName)
        {
            throw new NotImplementedException();
        }
    }
}
