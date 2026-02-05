using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Prism.Mvvm;

namespace WebFeedReader.Models;

using System;

public sealed class FeedItem : BindableBase
{
    private bool isNg;

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

    public bool IsRead { get; set; }

    public bool IsFavorite { get; set; }

    public int NgWordCheckVersion { get; set; }

    public bool IsNg { get => isNg; set => SetProperty(ref isNg, value); }
}