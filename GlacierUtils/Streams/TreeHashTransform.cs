using System;
using System.Collections.Generic;
using System.Security.Cryptography;

namespace GlacierUtils.Streams
{
    /// <summary>
    /// An ICryptoTransform implementation for calculating a SHA256 tree hash of configurably sized chunks
    /// of the input stream. The algorithm calculates the result incrementally and avoids storing all of
    /// the individual chunk hashes during calculation.
    /// </summary>
    public class TreeHashTransform : ICryptoTransform
    {
        private readonly SHA256Managed _hash = new SHA256Managed();
        private readonly Stack<byte[]> _hashes = new Stack<byte[]>();
        private readonly int _chunkSize;
        private readonly int _hashSize;
        private int _total;
        private bool _transformedFinalBlock;
        private bool _disposed;

        public int InputBlockSize { get { return _chunkSize; } }
        public int OutputBlockSize { get { return _chunkSize; } }
        public bool CanTransformMultipleBlocks { get { return true; } }
        public bool CanReuseTransform { get { return false; } }

        public byte[] TreeHash
        {
            get
            {
                if (_disposed) throw new ObjectDisposedException("Object has been disposed");
                if (!_transformedFinalBlock) throw new InvalidOperationException("Final block has not been flushed");
                if (_total == 0) return _hash.ComputeHash(new byte[0]);
                var items = new Queue<byte[]>(_hashes);
                var result = items.Dequeue();
                while (items.Count > 0)
                {
                    result = Combine(items.Dequeue(), result);
                }
                return result;
            }
        }

        /// <param name="chunkSize">The size of the chunks used for calculating hashes</param>
        public TreeHashTransform(int chunkSize)
        {
            _chunkSize = chunkSize;
            _hashSize = _hash.HashSize / 8;
        }

        public void Dispose()
        {
            _hash.Dispose();
            _disposed = true;
        }

        public int TransformBlock(byte[] inputBuffer, int inputOffset, int inputCount, byte[] outputBuffer, int outputOffset)
        {
            if (_disposed) throw new ObjectDisposedException("Object has been disposed");
            if (_transformedFinalBlock) throw new InvalidOperationException("Final block has already been transformed");
            if (inputCount % _chunkSize != 0) throw new ArgumentException("Input count is not a multiple of the block size", "inputCount");
            if (inputBuffer.Length < inputOffset + inputCount) throw new ArgumentException("Insufficient data in input buffer");
            if (outputBuffer.Length < outputOffset + inputCount) throw new ArgumentException("Insufficient space in output buffer");

            for (var i = 0; i < inputCount; i += _chunkSize)
            {
                Add(_hash.ComputeHash(inputBuffer, inputOffset + i, _chunkSize));
            }

            Buffer.BlockCopy(inputBuffer, inputOffset, outputBuffer, outputOffset, inputCount);
            return inputCount;
        }

        public byte[] TransformFinalBlock(byte[] inputBuffer, int inputOffset, int inputCount)
        {
            if (_disposed) throw new ObjectDisposedException("Object has been disposed");
            if (_transformedFinalBlock) throw new InvalidOperationException("Final block has already been transformed");
            if (inputBuffer.Length < inputOffset + inputCount) throw new ArgumentException("Insufficient data in input buffer");
            _transformedFinalBlock = true;
            var finalBlockSize = inputCount % _chunkSize;
            for (var i = 0; i < inputCount - finalBlockSize; i += _chunkSize)
            {
                Add(_hash.ComputeHash(inputBuffer, inputOffset + i, _chunkSize));
            }
            if (finalBlockSize > 0)
            {
                Add(_hash.ComputeHash(inputBuffer, inputCount - finalBlockSize, finalBlockSize));
            }
            var result = new byte[inputCount];
            Buffer.BlockCopy(inputBuffer, inputOffset, result, 0, inputCount);
            return result;
        }

        private void Add(byte[] hash)
        {
            _hashes.Push(hash);
            var count = ++_total;
            while (count % 2 == 0)
            {
                count >>= 1;
                var r = _hashes.Pop();
                var l = _hashes.Pop();
                _hashes.Push(Combine(l, r));
            }
        }

        private byte[] Combine(byte[] left, byte[] right)
        {
            var input = new byte[_hashSize * 2];
            Buffer.BlockCopy(left, 0, input, 0, _hashSize);
            Buffer.BlockCopy(right, 0, input, _hashSize, _hashSize);
            return _hash.ComputeHash(input);
        }
    }
}
