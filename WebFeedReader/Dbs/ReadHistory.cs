using System;

namespace WebFeedReader.Dbs
{
    public class ReadHistory
    {
        public int Id { get; set; }

        public int FeedItemId { get; set; } // FeedItemへの外部キー

        public DateTime ReadAt { get; set; }
    }
}