using WebFeedReader.Factories;

namespace WebFeedReader.Tests.Factories
{
    [TestFixture]
    public sealed class FeedSourceFactoryTests
    {
        [Test]
        public void FromJson_ValidJson_ReturnsFeedSource()
        {
            // arrange
            const string json = """
                                [
                                  {
                                    "id": 1,
                                    "name": "Example Site",
                                    "url": "https://example.com/rss",
                                    "enabled": true,
                                    "check_interval_minutes": 60,
                                    "updated_at": "2026-01-31T09:00:00",
                                    "created_at": "2026-01-01T12:00:00"
                                  }
                                ]
                                """;

            // act
            var sources = FeedSourceFactory.FromJson(json);

            // assert
            Assert.That(sources, Is.Not.Null);
            Assert.That(sources, Has.Count.EqualTo(1));

            var source = sources.First();
            Assert.Multiple(() =>
            {
                Assert.That(source.Id, Is.EqualTo(1));
                Assert.That(source.Name, Is.EqualTo("Example Site"));
                Assert.That(source.Url, Is.EqualTo(new Uri("https://example.com/rss")));
                Assert.That(source.Enabled, Is.True);
                Assert.That(source.CheckIntervalMinutes, Is.EqualTo(60));
                Assert.That(
                    source.UpdatedAt,
                    Is.EqualTo(new DateTime(2026, 1, 31, 9, 0, 0)));
                Assert.That(
                    source.CreatedAt,
                    Is.EqualTo(new DateTime(2026, 1, 1, 12, 0, 0)));

                // Raw が保持されていることの最低限確認
                Assert.That(
                    source.Raw.ValueKind,
                    Is.Not.EqualTo(System.Text.Json.JsonValueKind.Undefined));
            });
        }
    }
}