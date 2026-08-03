using System;
using System.Text.Json.Serialization;

namespace WebFeedReader.Models
{
    public class ReadFlagHistory
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("feed_id")]
        public int FeedId { get; set; }

        [JsonPropertyName("updated_date")]
        public DateTime UpdatedDate { get; set; }
    }
}