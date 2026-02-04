using WebFeedReader.Models;

namespace WebFeedReader.Dbs
{
    using Microsoft.EntityFrameworkCore;

    public sealed class AppDbContext : DbContext
    {
        private readonly string dbPath;

        public AppDbContext(string dbPath)
        {
            this.dbPath = dbPath;
        }

        public DbSet<FeedItem> FeedItems => Set<FeedItem>();

        public DbSet<FeedSource> FeedSources => Set<FeedSource>();

        public DbSet<NgWord> NgWords => Set<NgWord>();

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            options.UseSqlite($"Data Source={dbPath}");
        }
    }
}