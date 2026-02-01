using System;
using System.Text.Json;

namespace WebFeedReader.Models
{
    public sealed class FeedSource
    {
            public int Id { get; init; }

            public string Name { get; init; } = string.Empty;

            public Uri Url { get; init; } = null!;

            public bool Enabled { get; init; }

            public int CheckIntervalMinutes { get; init; }

            public DateTime UpdatedAt { get; init; }

            public DateTime CreatedAt { get; init; }

            // API 生データ保持用
            public JsonElement Raw { get; init; }
    }
}