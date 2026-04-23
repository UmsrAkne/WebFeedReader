using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WebFeedReader.Models;

namespace WebFeedReader.Dbs
{
    public sealed class ReadHistoryRepository : IReadHistoryRepository
    {
        private readonly Func<AppDbContext> dbFactory;

        public ReadHistoryRepository(Func<AppDbContext> dbFactory)
        {
            this.dbFactory = dbFactory;
        }

        public async Task AddAsync(ReadHistory history)
        {
            await using var db = dbFactory();
            db.ReadHistories.Add(history);
            await db.SaveChangesAsync();
        }

        public async Task<IReadOnlyList<ReadHistory>> GetAllAsync()
        {
            await using var db = dbFactory();
            return await db.ReadHistories
                .AsNoTracking()
                .OrderByDescending(x => x.ReadAt)
                .ToListAsync();
        }
    }
}