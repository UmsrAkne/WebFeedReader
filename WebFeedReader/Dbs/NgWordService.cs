using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WebFeedReader.Api;
using WebFeedReader.Models;
using WebFeedReader.Utils;

namespace WebFeedReader.Dbs
{
    public sealed class NgWordService
    {
        private readonly Func<AppDbContext> dbFactory;
        private readonly AppSettings appSettings;
        private readonly IApiClient apiClient;

        public NgWordService(Func<AppDbContext> dbFactory, IApiClient apiClient)
        {
            this.dbFactory = dbFactory;
            appSettings = AppSettings.Load();
            this.apiClient = apiClient;
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

        public async Task SyncNgWordsAsync(int version)
        {
            var list = await apiClient.GetNgWordsAsync(version);
            foreach (var ngWord in list)
            {
                await AddNgWordAsync(ngWord);
            }
        }

        public async Task SyncNgWordsFromServerAsync()
        {
            var currentVersion = appSettings.ServerNgWordListVersion;
            var words = await apiClient.GetNgWordsAsync(currentVersion);
            var wordList = words.ToList();
            if (!wordList.Any())
            {
                return;
            }

            await using var db = dbFactory();

            foreach (var word in wordList)
            {
                // 既存があれば更新、なければ追加
                var existing = await db.NgWords.FindAsync(word.Id);
                if (existing == null)
                {
                    db.NgWords.Add(word);
                }
            }

            var latestVersion = wordList.Max(w => w.CreatedAt);
            appSettings.ServerNgWordListVersion = latestVersion;

            await db.SaveChangesAsync();
            await appSettings.SaveAsync();
        }

        private static bool ContainsNgWord(FeedItem feed, IReadOnlyList<string> ngWords)
        {
            return ngWords.Any(word => feed.Title.Contains(word) || feed.Summary.Contains(word));
        }
    }
}