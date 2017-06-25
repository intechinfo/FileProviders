using System;
using System.IO;

namespace Intech.FileProviders.GitFileProvider
{
    class ReadStreamDecorator : Stream
    {
        Stream _innerStream;
        RepositoryWrapper _rw;

        public ReadStreamDecorator(Stream innerStream, RepositoryWrapper rw)
        {
            _innerStream = innerStream;
            _rw = rw;
            _rw.StreamWrapperCount++;
        }

        public override bool CanRead => _innerStream.CanRead;

        public override bool CanSeek => _innerStream.CanSeek;

        public override bool CanWrite => _innerStream.CanWrite;

        public override long Length => _innerStream.Length;

        public override long Position { get => _innerStream.Position; set => _innerStream.Position = value; }

        public override void Flush()
        {
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            return _innerStream.Read(buffer, offset, count);
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            return _innerStream.Seek(offset, origin);
        }

        public override void SetLength(long value)
        {
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
        }

        public void Dispose()
        {
            _rw.Close();
            _innerStream.Dispose();
        }
    }
}
