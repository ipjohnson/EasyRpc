using System;
using System.Collections.Generic;
using System.Text;

namespace EasyRpc.Abstractions.Path
{
    public interface IBasePathAttribute
    {
        string BasePath { get; }
    }

    public class BasePathAttribute : Attribute, IBasePathAttribute
    {
        public BasePathAttribute(string basePath)
        {
            BasePath = basePath;
        }

        public string BasePath { get; }
    }
}
