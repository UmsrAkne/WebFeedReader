using System;
using System.Text.Json.Serialization;

namespace WebFeedReader.Models
{
    internal sealed class FeedItemDto
    {
        [JsonPropertyName("id")]
        public int Id { get; init; }

        [JsonPropertyName("title")]
        public string Title { get; init; }

        [JsonPropertyName("link")]
        public string Link { get; init; }

        [JsonPropertyName("published")]
        public DateTimeOffset? Published { get; init; }

        [JsonPropertyName("summary")]
        public string Summary { get; init; }

        [JsonPropertyName("source_id")]
        public int SourceId { get; init; }
    }
}