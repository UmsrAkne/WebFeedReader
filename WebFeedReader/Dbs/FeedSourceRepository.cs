using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using WebFeedReader.Models;

namespace WebFeedReader.Dbs
{
    public class FeedSourceRepository : IFeedSourceRepository
    {
        private readonly AppDbContext db;

        public FeedSourceRepository(AppDbContext db)
        {
            this.db = db;
        }

        public async Task UpsertAsync(FeedSource source)
        {
            const string sql = """
                               INSERT INTO FeedSources (
                                   Id,
                                   Name,
                                   Url,
                                   Enabled,
                                   CheckIntervalMinutes,
                                   UpdatedAt,
                                   CreatedAt,
                                   Raw
                               )
                               VALUES (
                                   @Id,
                                   @Name,
                                   @Url,
                                   @Enabled,
                                   @CheckIntervalMinutes,
                                   @UpdatedAt,
                                   @CreatedAt,
                                   @Raw
                               )
                               ON CONFLICT(Id) DO UPDATE SET
                                   Name = excluded.Name,
                                   Url = excluded.Url,
                                   Enabled = excluded.Enabled,
                                   CheckIntervalMinutes = excluded.CheckIntervalMinutes,
                                   UpdatedAt = excluded.UpdatedAt,
                                   Raw = excluded.Raw;
                               """;

            await db.Database.ExecuteSqlRawAsync(
                sql,
                new SqliteParameter("@Id", source.Id),
                new SqliteParameter("@Name", source.Name),
                new SqliteParameter("@Url", source.Url.ToString()),
                new SqliteParameter("@Enabled", source.Enabled),
                new SqliteParameter("@CheckIntervalMinutes", source.CheckIntervalMinutes),
                new SqliteParameter("@UpdatedAt", source.UpdatedAt),
                new SqliteParameter("@CreatedAt", source.CreatedAt),
                new SqliteParameter("@Raw", source.Raw));
        }

        public async Task<IReadOnlyList<FeedSource>> GetAllAsync()
        {
            return await db.FeedSources
                .AsNoTracking()
                .OrderBy(x => x.Name)
                .ToListAsync();
        }
    }
}