using System;
using System.Security.Cryptography;
using GlacierUtils.Streams;

namespace GlacierUtils
{
    /// <summary>
    /// A SHA256 tree hash calculator that can be used to calculate the resulting tree hash from the chunk
    /// hashes calculated by e.g. <see cref="ChunkHashTransform"/>.
    /// </summary>
    public static class TreeHashCalculator
    {
        /// <summary>
        /// Calculate the SHA256 tree hash from a set of chunk hashes
        /// </summary>
        /// <param name="chunkHashes">The SHA256 hashes of individual chunks</param>
        /// <returns>The SHA256 tree hash</returns>
        public static byte[] CalculateTreeHash(byte[][] chunkHashes)
        {
            return CalculateTreeHash(chunkHashes, 0, chunkHashes.Length);
        }

        /// <summary>
        /// Calculate the SHA256 tree hash from a subset of the input chunk hashes.
        /// </summary>
        /// <remarks>
        /// This method does not validate whether <paramref name="startIndex"/> and <paramref name="count"/>
        /// form a valid tree hash aligned range.
        /// </remarks>
        /// <param name="chunkHashes">The SHA256 hashes of individual chunks</param>
        /// <param name="startIndex">The starting index to perform the tree hash calculation</param>
        /// <param name="count">The number of chunk hashes to be used for calculating the tree hash</param>
        /// <returns>The SHA256 tree hash</returns>
        public static byte[] CalculateTreeHash(byte[][] chunkHashes, int startIndex, int count)
        {
            var numHashes = chunkHashes.Length;
            if (startIndex < 0 || startIndex >= numHashes) throw new ArgumentOutOfRangeException("startIndex", "StartIndex must be non-negative and less than the number of hashes");
            if (count <= 0) throw new ArgumentOutOfRangeException("count", "Count must be positive");
            if (startIndex + count > numHashes) throw new ArgumentException("Invalid StartIndex or Count");

            using (var hash = new SHA256Managed())
            {
                var hashSize = hash.HashSize / 8;
                foreach (var chunkHash in chunkHashes)
                {
                    if (chunkHash == null) throw new ArgumentException("Input hashes must not be null");
                    if (chunkHash.Length != hashSize) throw new ArgumentException("Invalid hash size in input");
                }
                var thisLevel = chunkHashes;
                var inputOffset = startIndex;
                var inputHashCount = count;
                while (inputHashCount > 1)
                {
                    var nextLevel = new byte[(inputHashCount + 1) / 2][];
                    var hashInput = new byte[hashSize * 2];
                    var srcOffset = inputOffset;
                    var dstOffset = 0;
                    var pairCount = inputHashCount / 2;
                    for (var i = 0; i < pairCount; i++)
                    {
                        Buffer.BlockCopy(thisLevel[srcOffset++], 0, hashInput, 0, hashSize);
                        Buffer.BlockCopy(thisLevel[srcOffset++], 0, hashInput, hashSize, hashSize);
                        nextLevel[dstOffset++] = hash.ComputeHash(hashInput);
                    }
                    if (inputHashCount % 2 != 0)
                    {
                        nextLevel[dstOffset] = thisLevel[srcOffset];
                    }
                    thisLevel = nextLevel;
                    inputOffset = 0;
                    inputHashCount = thisLevel.Length;
                }

                var result = new byte[hashSize];
                Buffer.BlockCopy(thisLevel[inputOffset], 0, result, 0, hashSize);
                return result;
            }
        }
    }
}