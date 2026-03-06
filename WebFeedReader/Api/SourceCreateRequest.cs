using System.Text.Json.Serialization;

namespace WebFeedReader.Api
{
    public class SourceCreateRequest
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("url")]
        public string Url { get; set; }

        [JsonPropertyName("check_interval_minutes")]
        public int CheckIntervalMinutes { get; set; }
    }
}