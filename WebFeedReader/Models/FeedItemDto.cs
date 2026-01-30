using System;
using System.Text.Json.Serialization;

namespace WebFeedReader.Models
{
    internal sealed class FeedItemDto
    {
        [JsonPropertyName("title")]
        public string Title { get; init; }

        [JsonPropertyName("link")]
        public string Link { get; init; }

        [JsonPropertyName("published")]
        public DateTimeOffset? Published { get; init; }

        [JsonPropertyName("summary")]
        public string Summary { get; init; }
    }
}