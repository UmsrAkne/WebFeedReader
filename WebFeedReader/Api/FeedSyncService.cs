using System;
using System.Linq;
using System.Threading.Tasks;
using Serilog;
using WebFeedReader.Dbs;
using WebFeedReader.Factories;
using WebFeedReader.Utils;

namespace WebFeedReader.Api
{
    public sealed class FeedSyncService : IFeedSyncService
    {
        private readonly IApiClient apiClient;
        private readonly IFeedItemRepository repository;
        private readonly NgWordService ngWordService;

        public FeedSyncService(IApiClient apiClient, IFeedItemRepository repository, NgWordService ngWordService)
        {
            this.apiClient = apiClient;
            this.repository = repository;
            this.ngWordService = ngWordService;
        }

        public async Task SyncAsync(DateTimeOffset since)
        {
            var json = await apiClient.GetFeedsAsync(since);
            json = DateTimeFormatFixer.FixDateTimeFormat(json);
            var feeds = FeedItemFactory.FromJson(json, string.Empty);
            var checkResults = await ngWordService.Check(feeds);

            foreach (var ngCheckResult in checkResults)
            {
                var feed = feeds.FirstOrDefault(c => c.Id == ngCheckResult.FeedId);
                if (feed != null)
                {
                    feed.IsNg = ngCheckResult.IsNg;
                    feed.NgWordCheckVersion = ngCheckResult.Version;
                }
            }

            foreach (var feed in feeds)
            {
                await repository.UpsertAsync(feed);
            }

            Log.Information("Feeds synced. {@feedInfo}", new { feeds.Count, });
        }
    }
}