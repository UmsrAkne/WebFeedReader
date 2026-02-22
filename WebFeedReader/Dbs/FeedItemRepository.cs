using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using WebFeedReader.Models;

namespace WebFeedReader.Dbs
{
    public sealed class FeedItemRepository : IFeedItemRepository
    {
        private readonly Func<AppDbContext> dbFactory;

        public FeedItemRepository(Func<AppDbContext> dbFactory)
        {
            this.dbFactory = dbFactory;
        }

        public async Task UpsertAsync(FeedItem item)
        {
            const string sql = """
                               INSERT INTO FeedItems (
                                   Id, 
                                   Key,
                                   SourceId,
                                   SourceName,
                                   Title,
                                   Link, 
                                   Published,
                                   Summary,
                                   Raw,
                                   IsRead,
                                   IsFavorite,
                                   IsNg,
                                   NgWordCheckVersion
                               )
                               VALUES (
                                   @Id,
                                   @Key,
                                   @SourceId,
                                   @SourceName,
                                   @Title,
                                   @Link,
                                       
                                   @Published,
                                   @Summary,
                                   @Raw,
                                   @IsRead,
                                   @IsFavorite,
                                   @IsNg,
                                   @NgWordCheckVersion
                               )
                               ON CONFLICT DO NOTHING;
                               """;

            await using var db = dbFactory();
            await db.Database.ExecuteSqlRawAsync(
                sql,
                new SqliteParameter("@Id", item.Id),
                new SqliteParameter("@Key", item.Key),
                new SqliteParameter("@SourceId", item.SourceId),
                new SqliteParameter("@SourceName", item.SourceName),
                new SqliteParameter("@Title", item.Title),
                new SqliteParameter("@Link", item.Link),
                new SqliteParameter("@Published", item.Published),
                new SqliteParameter("@Summary", item.Summary),
                new SqliteParameter("@Raw", item.Raw),
                new SqliteParameter("@IsRead", item.IsRead),
                new SqliteParameter("@IsFavorite", item.IsFavorite),
                new SqliteParameter("@IsNg", item.IsNg),
                new SqliteParameter("@NgWordCheckVersion", item.NgWordCheckVersion));
        }

        public async Task<IReadOnlyList<FeedItem>> GetAllAsync()
        {
            await using var db = dbFactory();

            // 1. まずデータを取得する（この時点ではソートしない）
            var items = await db.FeedItems
                .AsNoTracking()
                .ToListAsync();

            // 2. メモリ上（C#側）で並べ替える
            return items
                .OrderByDescending(x => x.Published)
                .ToList();
        }

        public async Task<IReadOnlyList<FeedItem>> GetBySourceIdAsync(int sourceId)
        {
            await using var db = dbFactory();
            var items = await db.FeedItems
                .AsNoTracking()
                .Where(x => x.SourceId == sourceId)
                .ToListAsync();

            return items
                .OrderByDescending(x => x.Published)
                .ToList();
        }

        public async Task<IReadOnlyList<FeedItem>> GetBySourceIdPagedAsync(int sourceId, int offset, int limit, FeedSearchOption option)
        {
            await using var db = dbFactory();

            var baseQuery = db.FeedItems
                .AsNoTracking()
                .Where(x => x.SourceId == sourceId);

            if (option.IsUnreadOnly)
            {
                baseQuery = baseQuery.Where(x => !x.IsRead);
            }

            // SQLite は DateTimeOffset のソートを DB 側で処理できないため、
            // 一旦 List に展開してからメモリ上でソートを行う必要がある。
            // (See: SQLite Error: 'DateTimeOffset' in ORDER BY clauses)
            // また、ソートに必要な情報のみをメモリ上に載せることでパフォーマンスを上げる。
            var lightweight = await baseQuery
                .Select(x => new { x.Id, x.Published, })
                .ToListAsync();

            var orderedIds = (option.IsReverseOrder
                    ? lightweight.OrderBy(x => x.Published)
                    : lightweight.OrderByDescending(x => x.Published))
                .Skip(offset)
                .Take(limit)
                .Select(x => x.Id)
                .ToList();

            // 抽出した id を FeedItem に戻す。
            var items = await db.FeedItems
                .AsNoTracking()
                .Where(x => orderedIds.Contains(x.Id))
                .ToListAsync();

            var itemDict = items.ToDictionary(x => x.Id);

            // items の順序は保証されないため、 orderedIds の順序を復元する。
            return orderedIds
                .Select(id => itemDict[id])
                .ToList();
        }

        public async Task MarkAsReadAsync(IEnumerable<string> keys)
        {
            var keyList = keys.Distinct().ToList();
            if (keyList.Count == 0)
            {
                return;
            }

            await using var db = dbFactory();

            // 900件ずつのチャンクに分けて実行
            // SQLiteの IN句は、1000件前後に達すると制限があり、その対策コード。
            foreach (var chunk in keyList.Chunk(900))
            {
                await db.FeedItems
                    .Where(f => chunk.Contains(f.Key))
                    .ExecuteUpdateAsync(s => s.SetProperty(f => f.IsRead, true));
            }
        }

        public async Task MarkAsFavoriteAsync(string key, bool isFavorite)
        {
            await using var db = dbFactory();

            var item = await db.FeedItems.FirstOrDefaultAsync(f => f.Key == key);

            if (item == null)
            {
                return;
            }

            item.IsFavorite = isFavorite;

            await db.SaveChangesAsync();
        }

        public async Task ApplyNgCheckResultsAsync(IEnumerable<NgCheckResult> results)
        {
            var resultList = results.ToList();
            if (resultList.Count == 0)
            {
                return;
            }

            await using var db = dbFactory();

            // FeedId ごとにまとめておく（IN 句用）
            // SQLite の IN 句制限対策で 900 件ずつ処理
            foreach (var chunk in resultList.Chunk(900))
            {
                var version = chunk.First().Version;

                var ngIds = chunk.Where(r => r.IsNg).Select(r => r.FeedId).ToList();
                var okIds = chunk.Where(r => !r.IsNg).Select(r => r.FeedId).ToList();

                if (ngIds.Count > 0)
                {
                    await db.FeedItems
                        .Where(f => ngIds.Contains(f.Id))
                        .ExecuteUpdateAsync(s =>
                            s.SetProperty(f => f.IsNg, true)
                                .SetProperty(f => f.NgWordCheckVersion, version));
                }

                if (okIds.Count > 0)
                {
                    await db.FeedItems
                        .Where(f => okIds.Contains(f.Id))
                        .ExecuteUpdateAsync(s =>
                            s.SetProperty(f => f.IsNg, false)
                                .SetProperty(f => f.NgWordCheckVersion, version));
                }
            }
        }
    }
}