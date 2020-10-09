using System;
using System.Collections.Generic;
using System.Text;

namespace EasyRpc.AspNetCore.Documentation
{
    public interface IStaticResource
    {
        byte[] Content { get; }
        string Path { get; }
        string ContentType { get; }
        bool IsBrCompressed { get; }
    }

    public class StaticResource : IStaticResource
    {
        public byte[] Content { get; set; }

        public string Path { get; set; }

        public string ContentType { get; set; }

        public bool IsBrCompressed { get; set; }
    }
}
