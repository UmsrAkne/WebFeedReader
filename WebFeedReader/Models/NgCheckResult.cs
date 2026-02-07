namespace WebFeedReader.Models
{
    public record NgCheckResult
    {
        public long FeedId { get; init; }

        public bool IsNg { get; init; }

        public int Version { get; init; }
    }
}