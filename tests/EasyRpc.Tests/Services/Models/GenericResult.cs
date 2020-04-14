using System;
using System.Collections.Generic;
using System.Text;

namespace EasyRpc.Tests.Services.Models
{
    public class GenericResult<T>
    {
        public T Result { get; set; }
    }
}
