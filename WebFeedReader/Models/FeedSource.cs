using System;
using System.ComponentModel.DataAnnotations.Schema;
using Prism.Mvvm;

namespace WebFeedReader.Models
{
    public sealed class FeedSource : BindableBase
    {
            private int unreadCount;

            public int Id { get; init; }

            public string Name { get; init; } = string.Empty;

            public Uri Url { get; init; } = null!;

            public bool Enabled { get; init; }

            public int CheckIntervalMinutes { get; init; }

            public DateTime UpdatedAt { get; init; }

            public DateTime CreatedAt { get; init; }

            // API 生データ保持用
            public string Raw { get; init; }

            [NotMapped]
            public int UnreadCount { get => unreadCount; set => SetProperty(ref unreadCount, value); }
    }
}