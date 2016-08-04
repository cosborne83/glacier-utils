using System;
using System.Collections.Generic;
using System.Security.Cryptography;

namespace GlacierUtils.Streams
{
    /// <summary>
    /// An ICryptoTransform implementation for calculating a set of SHA256 hashes of configurably sized 
    /// chunks of the input stream.
    /// </summary>
    public class ChunkHashTransform : ICryptoTransform
    {
        private readonly SHA256Managed _hash = new SHA256Managed();
        private readonly List<byte[]> _hashes = new List<byte[]>();
        private readonly int _chunkSize;
        private readonly int _hashSize;
        private bool _transformedFinalBlock;
        private bool _disposed;

        public int InputBlockSize { get { return _chunkSize; } }
        public int OutputBlockSize { get { return _chunkSize; } }
        public bool CanTransformMultipleBlocks { get { return true; } }
        public bool CanReuseTransform { get { return false; } }

        public byte[][] Hashes
        {
            get
            {
                if (_disposed) throw new ObjectDisposedException("Object has been disposed");
                if (!_transformedFinalBlock) throw new InvalidOperationException("Final block has not been flushed");
                var result = new byte[_hashes.Count][];
                for (var i = 0; i < _hashes.Count; i++)
                {
                    Buffer.BlockCopy(_hashes[i], 0, result[i] = new byte[_hashSize], 0, _hashSize);
                }
                return result;
            }
        }

        /// <param name="chunkSize">The size of the chunks used for calculating hashes</param>
        public ChunkHashTransform(int chunkSize)
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
                _hashes.Add(_hash.ComputeHash(inputBuffer, inputOffset + i, _chunkSize));
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
                _hashes.Add(_hash.ComputeHash(inputBuffer, inputOffset + i, _chunkSize));
            }
            if (finalBlockSize > 0)
            {
                _hashes.Add(_hash.ComputeHash(inputBuffer, inputCount - finalBlockSize, finalBlockSize));
            }
            var result = new byte[inputCount];
            Buffer.BlockCopy(inputBuffer, inputOffset, result, 0, inputCount);
            return result;
        }
    }
}
