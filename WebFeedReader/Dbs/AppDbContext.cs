using System;
using System.IO;
using WebFeedReader.Models;

namespace WebFeedReader.Dbs
{
    using Microsoft.EntityFrameworkCore;

    public sealed class AppDbContext : DbContext
    {
        private readonly string dbPath;

        public AppDbContext(string dbPath = "")
        {
            if (string.IsNullOrEmpty(dbPath))
            {
                var baseDir = AppContext.BaseDirectory;
                var path = Path.Combine(baseDir, "Feeds.db");
                dbPath = path;
            }

            this.dbPath = dbPath;
        }

        public DbSet<FeedItem> FeedItems => Set<FeedItem>();

        public DbSet<FeedSource> FeedSources => Set<FeedSource>();

        public DbSet<NgWord> NgWords => Set<NgWord>();

        public DbSet<ReadHistory> ReadHistories => Set<ReadHistory>();

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            options.UseSqlite($"Data Source={dbPath}");
        }
    }
}