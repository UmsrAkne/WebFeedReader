using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using WebFeedReader.Models;

namespace WebFeedReader.Factories
{
    public static class FeedItemFactory
    {
        public static IReadOnlyList<FeedItem> FromJson(string json, string sourceName)
        {
            using var doc = JsonDocument.Parse(json);

            return doc.RootElement
                .EnumerateArray()
                .Select(e => ConvertOne(e, sourceName))
                .ToList();
        }

        private static FeedItem ConvertOne(JsonElement element, string sourceName)
        {
            var dto = element.Deserialize<FeedItemDto>();

            return new FeedItem
            {
                Id = dto.Id,
                SourceId = dto.SourceId,
                SourceName = sourceName,
                Title = dto.Title,
                Link = dto.Link,
                Published = dto.Published,
                Summary = dto.Summary,
                Key = FeedItem.BuildKey(dto.SourceId, dto.Link),
                Raw = element.Clone().GetRawText(),
            };
        }
    }
}