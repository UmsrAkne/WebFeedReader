using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using WebFeedReader.Models;

namespace WebFeedReader.Factories
{
    public static class FeedSourceFactory
    {
        public static IReadOnlyList<FeedSource> FromJson(string json)
        {
            using var doc = JsonDocument.Parse(json);

            return doc.RootElement
                .EnumerateArray()
                .Select(ConvertOne)
                .ToList();
        }

        private static FeedSource ConvertOne(JsonElement element)
        {
            var dto = element.Deserialize<FeedSourceDto>()
                      ?? throw new JsonException("Failed to deserialize FeedSourceDto.");

            return new FeedSource
            {
                Id = dto.Id,
                Name = dto.Name,
                Url = new Uri(dto.Url),
                Enabled = dto.Enabled,
                CheckIntervalMinutes = dto.CheckIntervalMinutes,
                UpdatedAt = dto.UpdatedAt,
                CreatedAt = dto.CreatedAt,
                Raw = element.Clone(),
            };
        }
    }
}