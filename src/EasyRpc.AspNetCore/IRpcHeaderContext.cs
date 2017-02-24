using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EasyRpc.AspNetCore
{
    public interface IRpcHeaderContext
    {
        T GetValue<T>(string key = null);

        void SetValue<T>(T value, string key = null);
    }
}
