using WebFeedReader.Factories;
using WebFeedReader.Models;

namespace WebFeedReader.Tests.Factories
{
    [TestFixture]
    public sealed class FeedItemFactoryTests
    {
        [Test]
        public void FromJson_ValidJson_ReturnsFeedItems()
        {
            // Arrange
            const string json = """
                                [
                                  {
                                    "title": "サンプル記事A：架空サービスの新機能が発表",
                                    "link": "https://example.com/articles/sample-a?source=rss",
                                    "published": "2026-01-27T14:52:00",
                                    "source_id": 1
                                  },
                                  {
                                    "title": "サンプル記事B：テスト用データの扱い方について",
                                    "link": "https://example.com/articles/sample-b?source=rss",
                                    "published": "2026-01-27T14:46:51",
                                    "source_id": 2
                                  },
                                  {
                                    "title": "サンプル記事C：システム更新のお知らせ",
                                    "link": "https://example.com/articles/sample-c?source=rss",
                                    "published": "2026-01-27T14:40:37",
                                    "source_id": 3
                                  }
                                ]
                                """;

            const int sourceId = 1;
            const string sourceName = "Dummy Source";

            // Act
            var items = FeedItemFactory.FromJson(json, sourceName);

            // Assert
            Assert.That(items.Count, Is.EqualTo(3));

            var first = items.First();
            Assert.Multiple(() =>
            {
                Assert.That(first.SourceId, Is.EqualTo(sourceId));
                Assert.That(first.SourceName, Is.EqualTo(sourceName));
                Assert.That(first.Title, Is.EqualTo("サンプル記事A：架空サービスの新機能が発表"));
            });

            Assert.Multiple(() =>
            {
                Assert.That(
                    first.Link, Is.EqualTo("https://example.com/articles/sample-a?source=rss"));
                Assert.That(
                    first.Published, Is.EqualTo(new DateTimeOffset(2026, 1, 27, 14, 52, 0, TimeSpan.FromHours(9))));
                Assert.That(
                    first.Key, Is.EqualTo(FeedItem.BuildKey(sourceId, first.Link)));
            });

            Assert.That(first.Raw, Is.Not.Null);
        }
    }
}