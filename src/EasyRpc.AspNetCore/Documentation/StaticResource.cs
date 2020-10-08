using System;
using System.Collections.Generic;
using System.Text;

namespace EasyRpc.AspNetCore.Documentation
{
    public class StaticResource
    {
        private byte[] _bytes;

        public StaticResource(string contentType, bool isBrCompressed, byte[] bytes)
        {
            ContentType = contentType;
            IsBrCompressed = isBrCompressed;
            _bytes = bytes;
        }


        public string ContentType { get; }

        public bool IsBrCompressed { get; }

        public byte[] GetBytes()
        {
            return _bytes;
        }
    }
}
