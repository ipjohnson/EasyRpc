using System;
using System.Collections.Generic;
using System.Text;

namespace EasyRpc.AspNetCore.Errors
{
    public interface IErrorWrapper
    {
        string Message { get; set; }
    }
}
