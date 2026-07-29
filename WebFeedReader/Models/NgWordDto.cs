using System.Text.Json.Serialization;

namespace WebFeedReader.Models
{
    public class NgWordDto
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("word")]
        public string Word { get; set; }

        [JsonPropertyName("created_at")]
        public int CreatedAt { get; set;  }
    }
}