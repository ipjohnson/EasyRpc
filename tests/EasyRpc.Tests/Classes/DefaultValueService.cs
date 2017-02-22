using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EasyRpc.Tests.Classes
{
    public class DefaultValueService
    {
        public const string DefaultValueString = "-DefaultValue";

        public string SomeMethod(string baseString, string endString = DefaultValueString)
        {
            return baseString + endString;
        }
    }
}
