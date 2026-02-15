using System.ComponentModel.DataAnnotations.Schema;
using Prism.Mvvm;

namespace WebFeedReader.Models;

using System;

public sealed class FeedItem : BindableBase
{
    private bool isNg;
    private bool isRead;
    private int lineNumber;
    private bool isFavorite;

    public int Id { get; set; }

    /// <summary>
    /// クライアント内で安定して扱うためのキー。
    /// サーバーが entry の id を返さない/変わる可能性があるので、
    /// 最低限 (SourceId, Link) で一意にする前提。
    /// </summary>
    public string Key { get; set; }

    public int SourceId { get; set; }

    public string SourceName { get; set; }

    public string Title { get; set; }

    public string Link { get; set; }

    public DateTimeOffset? Published { get; set; }

    public string Summary { get; set; }

    public string Raw { get; set; }

    public static string BuildKey(int sourceId, string link)
        => $"{sourceId}:{link}";

    public bool IsRead { get => isRead; set => SetProperty(ref isRead, value); }

    public bool IsFavorite { get => isFavorite; set => SetProperty(ref isFavorite, value); }

    public int NgWordCheckVersion { get; set; }

    public bool IsNg { get => isNg; set => SetProperty(ref isNg, value); }

    [NotMapped]
    public int LineNumber { get => lineNumber; set => SetProperty(ref lineNumber, value); }
}