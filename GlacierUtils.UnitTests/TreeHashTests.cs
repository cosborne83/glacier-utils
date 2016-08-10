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
            var chunkHash = new ChunkHashTransform(ChunkSize);
            chunkHash.TransformFinalBlock(input, 0, input.Length);
            var hashes = chunkHash.Hashes;
            Assert.AreEqual(1, hashes.Length);
            var result1 = TreeHashCalculator.CalculateTreeHash(hashes);

            var treeHashCalculator = new TreeHashTransform(ChunkSize);
            treeHashCalculator.TransformFinalBlock(input, 0, input.Length);
            var result2 = treeHashCalculator.TreeHash;

            Assert.AreEqual(result1, result2);
        }

        [Test]
        public void TestIncompleteFinalBlock()
        {
            var input = new byte[ChunkSize / 2];
            var chunkHash = new ChunkHashTransform(ChunkSize);
            chunkHash.TransformFinalBlock(input, 0, input.Length);
            var hashes = chunkHash.Hashes;
            Assert.AreEqual(1, hashes.Length);
            var result1 = TreeHashCalculator.CalculateTreeHash(hashes);

            var treeHashCalculator = new TreeHashTransform(ChunkSize);
            treeHashCalculator.TransformFinalBlock(input, 0, input.Length);
            var result2 = treeHashCalculator.TreeHash;

            Assert.AreEqual(result1, result2);
        }

        [Test]
        public void TestMultiBlock()
        {
            var input = new byte[ChunkSize];
            var output = new byte[ChunkSize];
            var chunkHash = new ChunkHashTransform(ChunkSize);
            const int blockCount = 10;
            for (var i = 0; i < blockCount; i++)
            {
                input[0] = (byte)i;
                chunkHash.TransformBlock(input, 0, input.Length, output, 0);
            }
            chunkHash.TransformFinalBlock(input, 0, 0);
            var hashes = chunkHash.Hashes;
            Assert.AreEqual(blockCount, hashes.Length);
            var result1 = TreeHashCalculator.CalculateTreeHash(hashes);

            var treeHashCalculator = new TreeHashTransform(ChunkSize);
            for (var i = 0; i < blockCount; i++)
            {
                input[0] = (byte)i;
                treeHashCalculator.TransformBlock(input, 0, input.Length, output, 0);
            }
            treeHashCalculator.TransformFinalBlock(input, 0, 0);
            var result2 = treeHashCalculator.TreeHash;

            Assert.AreEqual(result1, result2);
        }
    }
}
