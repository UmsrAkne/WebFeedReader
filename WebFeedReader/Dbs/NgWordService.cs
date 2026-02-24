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

        /// <summary>
        /// NGワードを1件追加します。
        /// </summary>
        /// <param name="word">追加するNGワード。Value が空白のみの場合は追加されません。</param>
        /// <returns>
        /// 追加に成功した場合は true。既に存在する、または無効な値の場合は false。
        /// </returns>
        /// <remarks>
        /// 追加に成功すると AppSettings.NgWordListVersion をインクリメントし、永続化します。
        /// </remarks>
        public async Task<bool> AddNgWordAsync(NgWord word)
        {
            await using var db = dbFactory();
            if (string.IsNullOrWhiteSpace(word.Value))
            {
                return false;
            }

            word.Value = word.Value.Trim();
            var all = db.NgWords.Select(w => w.Value).ToList();
            if (all.Contains(word.Value))
            {
                return false;
            }

            await db.NgWords.AddAsync(word);
            await db.SaveChangesAsync();

            appSettings.NgWordListVersion++;
            await appSettings.SaveAsync();

            return true;
        }

        private static bool ContainsNgWord(FeedItem feed, IReadOnlyList<string> ngWords)
        {
            return ngWords.Any(word => feed.Title.Contains(word) || feed.Summary.Contains(word));
        }
    }
}