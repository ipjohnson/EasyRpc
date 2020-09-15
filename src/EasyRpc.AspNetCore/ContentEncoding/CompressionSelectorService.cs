using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace EasyRpc.AspNetCore.ContentEncoding
{
    public interface ICompressionSelectorService
    {
        bool ShouldCompressResult(IEndPointMethodConfigurationReadOnly configuration);
    }

    public class CompressionSelectorService : ICompressionSelectorService
    {
        public bool ShouldCompressResult(IEndPointMethodConfigurationReadOnly configuration)
        {
            return !configuration.ReturnType.IsValueType;
        }
    }
}
