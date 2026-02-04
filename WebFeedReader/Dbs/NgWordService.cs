using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WebFeedReader.Models;
using WebFeedReader.Utils;

namespace WebFeedReader.Dbs
{
    public sealed class NgWordService
    {
        // SQLite 同時アクセス防止
        private readonly static SemaphoreSlim DbLock = new (1, 1);

        private readonly string dbPath;
        private readonly AppSettings appSettings;

        public NgWordService(string dbPath, AppSettings appSettings)
        {
            this.dbPath = dbPath;
            this.appSettings = appSettings;
        }

        /// <summary>
        /// 新着フィード用：未判定のものだけ NG チェックして返す
        /// </summary>
        /// <param name="feeds">チェックするフィードのリスト</param>
        /// <returns>非同期処理を表す Task</returns>
        public async Task<IReadOnlyList<FeedItem>> FilterNewFeedsAsync(IEnumerable<FeedItem> feeds)
        {
            var list = feeds.ToList();
            await EnsureCheckedAsync(list);
            return list.Where(f => !f.IsNg).ToList();
        }

        /// <summary>
        /// 表示直前などで呼ぶ：必要なものだけ NG チェックする
        /// </summary>
        /// <param name="feeds">チェックするフィードのリスト</param>
        /// <returns>非同期処理を表す Task</returns>
        public async Task EnsureCheckedAsync(IEnumerable<FeedItem> feeds)
        {
            var targetVersion = appSettings.NgWordListVersion;
            var targets = feeds
                .Where(f => f.NgWordCheckVersion != targetVersion)
                .ToList();

            if (targets.Count == 0)
            {
                return;
            }

            await DbLock.WaitAsync();
            try
            {
                await using var db = new AppDbContext(dbPath);

                var ngWords = await db.NgWords
                    .Select(w => w.Value)
                    .ToListAsync();

                foreach (var feed in targets)
                {
                    feed.IsNg = ContainsNgWord(feed, ngWords);
                    feed.NgWordCheckVersion = targetVersion;
                }

                await db.SaveChangesAsync();
            }
            finally
            {
                DbLock.Release();
            }
        }

        private static bool ContainsNgWord(FeedItem feed, IReadOnlyList<string> ngWords)
        {
            return ngWords.Any(word => feed.Title.Contains(word) || feed.Summary.Contains(word));
        }
    }
}