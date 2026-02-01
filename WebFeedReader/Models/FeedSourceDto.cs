using System;
using System.Text.Json.Serialization;

namespace WebFeedReader.Models
{
    public sealed class FeedSourceDto
    {
        [JsonPropertyName("id")]
        public int Id { get; init; }

        [JsonPropertyName("name")]
        public string Name { get; init; } = string.Empty;

        [JsonPropertyName("url")]
        public string Url { get; init; } = string.Empty;

        [JsonPropertyName("enabled")]
        public bool Enabled { get; init; }

        [JsonPropertyName("check_interval_minutes")]
        public int CheckIntervalMinutes { get; init; }

        [JsonPropertyName("updated_at")]
        public DateTime UpdatedAt { get; init; }

        [JsonPropertyName("created_at")]
        public DateTime CreatedAt { get; init; }
    }
}