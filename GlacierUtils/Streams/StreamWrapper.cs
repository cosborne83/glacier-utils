using System.IO;

namespace GlacierUtils.Streams
{
    /// <summary>
    /// A stream implementation to wrap an underlying stream and optionally flush, rather than close
    /// the inner stream when the wrapper is disposed
    /// </summary>
    public class StreamWrapper : Stream
    {
        private readonly Stream _innerStream;
        private readonly bool _leaveOpen;

        public override bool CanRead { get { return _innerStream.CanRead; } }
        public override bool CanSeek { get { return _innerStream.CanSeek; } }
        public override bool CanWrite { get { return _innerStream.CanWrite; } }
        public override long Length { get { return _innerStream.Length; } }

        public override long Position
        {
            get { return _innerStream.Position; }
            set { _innerStream.Position = value; }
        }

        /// <param name="innerStream">The stream to be wrapped</param>
        /// <param name="leaveOpen">If true, <paramref name="innerStream"/> is flushed instead of closed when this wrapper
        /// is disposed</param>
        public StreamWrapper(Stream innerStream, bool leaveOpen)
        {
            _innerStream = innerStream;
            _leaveOpen = leaveOpen;
        }

        public override void Flush()
        {
            _innerStream.Flush();
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            return _innerStream.Seek(offset, origin);
        }

        public override void SetLength(long value)
        {
            _innerStream.SetLength(value);
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            return _innerStream.Read(buffer, offset, count);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            _innerStream.Write(buffer, offset, count);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (!_leaveOpen)
                {
                    _innerStream.Dispose();
                }
                else
                {
                    _innerStream.Flush();
                }
            }
            base.Dispose(disposing);
        }
    }
}
