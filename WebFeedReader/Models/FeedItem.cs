namespace WebFeedReader.Models;

using System;

public sealed record FeedItem
{
    public int Id { get; set; }

    /// <summary>
    /// クライアント内で安定して扱うためのキー。
    /// サーバーが entry の id を返さない/変わる可能性があるので、
    /// 最低限 (SourceId, Link) で一意にする前提。
    /// </summary>
    public string Key { get; init; }

    public int SourceId { get; init; }

    public string SourceName { get; init; }

    public string Title { get; init; }

    public string Link { get; init; }

    public DateTimeOffset? Published { get; init; }

    public string Summary { get; init; }

    public string Raw { get; init; }

    public static string BuildKey(int sourceId, string link)
        => $"{sourceId}:{link}";

    public bool IsRead { get; set; }

    public bool IsFavorite { get; set; }

    public int NgWordCheckVersion { get; set; }
}