using System;
using System.Threading.Tasks;
using WebFeedReader.Dbs;
using WebFeedReader.Factories;

namespace WebFeedReader.Api
{
    public sealed class FeedSyncService : IFeedSyncService
    {
        private readonly IApiClient apiClient;
        private readonly IFeedItemRepository repository;

        public FeedSyncService(IApiClient apiClient, IFeedItemRepository repository)
        {
            this.apiClient = apiClient;
            this.repository = repository;
        }

        public async Task SyncAsync(DateTime since)
        {
            var json = await apiClient.GetFeedsAsync(since);
            var feeds = FeedItemFactory.FromJson(json, string.Empty);

            foreach (var feed in feeds)
            {
                await repository.UpsertAsync(feed);
            }
        }
    }
}