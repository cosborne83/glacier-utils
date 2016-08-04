using System;
using System.IO;

namespace GlacierUtils.Streams
{
    /// <summary>
    /// A stream implementation to expose a limited range of an underlying stream
    /// </summary>
    public class RangeStream : Stream
    {
        private readonly Stream _innerStream;
        private readonly long _startOffset;
        private readonly long _length;

        public override bool CanRead { get { return _innerStream.CanRead; } }
        public override bool CanSeek { get { return _innerStream.CanSeek; } }
        public override bool CanWrite { get { return false; } }
        public override long Length { get { return _length; } }

        public override long Position
        {
            get { return _innerStream.Position - _startOffset; }
            set
            {
                var position = _startOffset + value;
                if (position < _startOffset) throw new ArgumentException("Seek before start of stream", "value");
                _innerStream.Position = position;
            }
        }

        /// <param name="innerStream">The stream to be exposed as a range</param>
        /// <param name="startOffset">The starting offset of the range</param>
        /// <param name="length">The length of the range</param>
        public RangeStream(Stream innerStream, long startOffset, long length)
        {
            if (innerStream == null) throw new ArgumentNullException("innerStream");
            if (startOffset < 0) throw new ArgumentOutOfRangeException("startOffset", "StartOffset must be non-negative");
            if (length < 0) throw new ArgumentOutOfRangeException("length", "Length must be non-negative");
            if (startOffset + length > innerStream.Length) throw new ArgumentException("Invalid StartOffset or Length");
            _innerStream = innerStream;
            _startOffset = startOffset;
            _length = length;
            _innerStream.Position = startOffset;
        }

        public override void Flush()
        {
            _innerStream.Flush();
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            long position;
            switch (origin)
            {
                case SeekOrigin.Begin:
                    position = offset;
                    break;
                case SeekOrigin.Current:
                    position = Position + offset;
                    break;
                case SeekOrigin.End:
                    position = Length + offset;
                    break;
                default:
                    throw new ArgumentException("Invalid origin", "origin");
            }

            return Position = position;
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException("Stream does not support writing");
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            if (buffer == null) throw new ArgumentNullException("buffer", "Buffer cannot be null");
            if (offset < 0) throw new ArgumentOutOfRangeException("offset", "Offset must be non-negative");
            if (count < 0) throw new ArgumentOutOfRangeException("count", "Count must be non-negative");
            if (offset + count > buffer.Length) throw new ArgumentException("Offset and length were out of bounds for the array or count is greater than the number of elements from index to the end of the source collection.");
            if (Position >= Length) return 0;
            if (Position + count > Length) count = (int)(Length - Position);
            return _innerStream.Read(buffer, offset, count);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException("Stream does not support writing");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _innerStream.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}