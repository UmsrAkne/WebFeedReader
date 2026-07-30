using WebFeedReader.Factories;
using NUnit.Framework;

namespace WebFeedReader.Tests.Factories
{
    [TestFixture]
    public sealed class FeedItemJsonTests
    {
        [Test]
        public void FromJson_WhenIsReadIsNumber_ReturnsCorrectBool()
        {
            // Arrange
            const string json = """
                                [
                                  {
                                    "id": 1,
                                    "title": "Test Item",
                                    "link": "https://example.com/1",
                                    "source_id": 1,
                                    "is_read": 1,
                                    "is_ng_word": 0
                                  },
                                  {
                                    "id": 2,
                                    "title": "Test Item 2",
                                    "link": "https://example.com/2",
                                    "source_id": 1,
                                    "is_read": 0,
                                    "is_ng_word": 1
                                  }
                                ]
                                """;

            // Act
            var items = FeedItemFactory.FromJson(json, "Test Source");

            // Assert
            Assert.That(items.Count, Is.EqualTo(2));
            Assert.Multiple(() =>
            {
                Assert.That(items[0].IsRead, Is.True);
                Assert.That(items[0].IsNg, Is.False);
                Assert.That(items[1].IsRead, Is.False);
                Assert.That(items[1].IsNg, Is.True);
            });
        }
    }
}