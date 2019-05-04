using GlacierUtils.Streams;
using NUnit.Framework;

namespace GlacierUtils.UnitTests
{
    [TestFixture]
    internal class TreeHashTests
    {
        private const int ChunkSize = 1024 * 1204;

        [Test]
        public void TestCompleteFinalBlock()
        {
            var input = new byte[ChunkSize];
            var chunkSha256 = new ChunkSha256Transform(ChunkSize);
            chunkSha256.TransformFinalBlock(input, 0, input.Length);
            var hashes = chunkSha256.Hashes;
            Assert.AreEqual(1, hashes.Length);
            var result1 = Sha256TreeHashCalculator.CalculateTreeHash(hashes);

            var sha256TreeHash = new Sha256TreeHashTransform(ChunkSize);
            sha256TreeHash.TransformFinalBlock(input, 0, input.Length);
            var result2 = sha256TreeHash.TreeHash;

            Assert.AreEqual(result1, result2);
        }

        [Test]
        public void TestIncompleteFinalBlock()
        {
            var input = new byte[ChunkSize / 2];
            var chunkSha256 = new ChunkSha256Transform(ChunkSize);
            chunkSha256.TransformFinalBlock(input, 0, input.Length);
            var hashes = chunkSha256.Hashes;
            Assert.AreEqual(1, hashes.Length);
            var result1 = Sha256TreeHashCalculator.CalculateTreeHash(hashes);

            var sha256TreeHash = new Sha256TreeHashTransform(ChunkSize);
            sha256TreeHash.TransformFinalBlock(input, 0, input.Length);
            var result2 = sha256TreeHash.TreeHash;

            Assert.AreEqual(result1, result2);
        }

        [Test]
        public void TestMultiBlock()
        {
            var input = new byte[ChunkSize];
            var output = new byte[ChunkSize];
            var chunkSha256 = new ChunkSha256Transform(ChunkSize);
            const int blockCount = 10;
            for (var i = 0; i < blockCount; i++)
            {
                input[0] = (byte)i;
                chunkSha256.TransformBlock(input, 0, input.Length, output, 0);
            }
            chunkSha256.TransformFinalBlock(input, 0, 0);
            var hashes = chunkSha256.Hashes;
            Assert.AreEqual(blockCount, hashes.Length);
            var result1 = Sha256TreeHashCalculator.CalculateTreeHash(hashes);

            var sha256TreeHash = new Sha256TreeHashTransform(ChunkSize);
            for (var i = 0; i < blockCount; i++)
            {
                input[0] = (byte)i;
                sha256TreeHash.TransformBlock(input, 0, input.Length, output, 0);
            }
            sha256TreeHash.TransformFinalBlock(input, 0, 0);
            var result2 = sha256TreeHash.TreeHash;

            Assert.AreEqual(result1, result2);
        }
    }
}
