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
                new SqliteParameter("@Published", (object?)item.Published ?? DBNull.Value),
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
            return await db.FeedItems
                .AsNoTracking()
                .OrderByDescending(x => x.Published)
                .ToListAsync();
        }

        public async Task<IReadOnlyList<FeedItem>> GetBySourceIdAsync(int sourceId)
        {
            await using var db = dbFactory();
            return await db.FeedItems
                .AsNoTracking()
                .Where(x => x.SourceId == sourceId)
                .OrderByDescending(x => x.Published)
                .ToListAsync();
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
    }
}