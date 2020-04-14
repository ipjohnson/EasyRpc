using System;
using System.Collections.Generic;
using System.Text;

namespace EasyRpc.Abstractions.Path
{
    public interface IPathAttribute
    {
        string Method { get; }

        string Path { get; }

        int SuccessCodeValue { get; }

        bool HasBody { get; }
    }
}
