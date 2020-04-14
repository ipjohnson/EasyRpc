using System;
using System.Collections.Generic;
using System.Text;

namespace EasyRpc.AspNetCore
{
    public interface IResultWrapper
    {

    }

    public interface IResultWrapper<T> : IResultWrapper
    {
        T Result { get; set; }
    }
}
