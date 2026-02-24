using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WebFeedReader.Models;
using WebFeedReader.Utils;

namespace WebFeedReader.Dbs
{
    public sealed class NgWordService
    {
        private readonly Func<AppDbContext> dbFactory;
        private readonly AppSettings appSettings;

        public NgWordService(Func<AppDbContext> dbFactory, AppSettings appSettings)
        {
            this.dbFactory = dbFactory;
            this.appSettings = appSettings;
        }

        public async Task<IReadOnlyList<NgCheckResult>> Check(IEnumerable<FeedItem> feeds)
        {
            await using var db = dbFactory();
            var ngWords = await db.NgWords.Select(w => w.Value).ToListAsync();
            return feeds.Select(f => new NgCheckResult
            {
                FeedId = f.Id,
                IsNg = f.NgWordCheckVersion < appSettings.NgWordListVersion ? ContainsNgWord(f, ngWords) : f.IsNg,
                Version = appSettings.NgWordListVersion,
            }).ToList();
        }

        public async Task<IEnumerable<NgWord>> GetAllNgWordsAsync()
        {
            await using var db = dbFactory();
            return await db.NgWords.ToListAsync();
        }

        private static bool ContainsNgWord(FeedItem feed, IReadOnlyList<string> ngWords)
        {
            return ngWords.Any(word => feed.Title.Contains(word) || feed.Summary.Contains(word));
        }
    }
}