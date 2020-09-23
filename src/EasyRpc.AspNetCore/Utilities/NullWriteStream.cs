using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EasyRpc.AspNetCore.Utilities
{
    public class NullWriteStream : Stream
    {
        private long _length;

        public override void Flush()
        {
            
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException();
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotImplementedException();
        }

        public override void SetLength(long value)
        {
            throw new NotImplementedException();
        }

        public override Task CopyToAsync(Stream destination, int bufferSize, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            _length += count - offset;

            return Task.CompletedTask;
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            _length += count - offset;
        }

        public override bool CanRead => false;

        public override bool CanSeek => false;
        
        public override bool CanWrite => true;

        public override long Length => _length;
        
        public override long Position { get; set; }
    }
}
