using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace EasyRpc.AspNetCore.ContentEncoding
{
    public interface ICompressionSelectorService
    {
        bool ShouldCompressResult(MethodInfo method);
    }

    public class CompressionSelectorService : ICompressionSelectorService
    {
        public bool ShouldCompressResult(MethodInfo method)
        {
            return true;
        }
    }
}
