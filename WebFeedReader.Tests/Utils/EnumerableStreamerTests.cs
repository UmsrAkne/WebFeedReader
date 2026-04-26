using WebFeedReader.Utils;

namespace WebFeedReader.Tests.Utils
{
    [TestFixture]
    public class EnumerableStreamerTests
    {
        [Test]
        public async Task GetChunkyStream_ShouldOutputAllItems()
        {
            // Arrange
            var source = Enumerable.Range(1, 100).ToList();
            var initialChunkSize = 10;
            var batchThreshold = 50;

            var result = new List<int>();

            // Act
            await foreach (var chunk in EnumerableStreamer.GetChunkyStream(source, initialChunkSize, batchThreshold))
            {
                result.AddRange(chunk);
            }

            // Assert
            Assert.That(result.Count, Is.EqualTo(source.Count));
            Assert.That(result, Is.EquivalentTo(source));
        }

        [Test]
        public async Task GetChunkyStream_WithEmptySource_ShouldOutputNothing()
        {
            // Arrange
            var source = new List<int>();
            var initialChunkSize = 10;
            var batchThreshold = 50;

            var result = new List<int>();

            // Act
            await foreach (var chunk in EnumerableStreamer.GetChunkyStream(source, initialChunkSize, batchThreshold))
            {
                result.AddRange(chunk);
            }

            // Assert
            Assert.That(result, Is.Empty);
        }

        [Test]
        public async Task GetChunkyStream_VerifyChunkingLogic()
        {
            // Arrange
            // 15個のアイテム
            // initialChunkSize = 2
            // batchThreshold = 10
            // チャンクサイズが 2, 3, 4, ... と増える
            // i=0: Take(2) -> i=2, chunkSize=3
            // i=2: Take(3) -> i=5, chunkSize=4
            // i=5: Take(4) -> i=9, chunkSize=5
            // i=9: Take(5) -> i=14, chunkSize=6
            // i=14: i >= batchThreshold(10) なので Skip(14).ToList() -> 1個

            var source = Enumerable.Range(1, 15).ToList();
            var initialChunkSize = 2;
            var batchThreshold = 10;

            var chunks = new List<List<int>>();

            // Act
            await foreach (var chunk in EnumerableStreamer.GetChunkyStream(source, initialChunkSize, batchThreshold))
            {
                chunks.Add(chunk);
            }

            // Assert
            Assert.That(chunks.Count, Is.EqualTo(5));
            Assert.That(chunks[0].Count, Is.EqualTo(2));
            Assert.That(chunks[1].Count, Is.EqualTo(3));
            Assert.That(chunks[2].Count, Is.EqualTo(4));
            Assert.That(chunks[3].Count, Is.EqualTo(5)); // i=9, Take(5) -> i=14
            Assert.That(chunks[4].Count, Is.EqualTo(1)); // i=14, Skip(14)

            var allItems = chunks.SelectMany(c => c).ToList();
            Assert.That(allItems, Is.EquivalentTo(source));
        }
    }
}