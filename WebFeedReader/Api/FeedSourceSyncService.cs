using System;
using System.Threading.Tasks;
using WebFeedReader.Dbs;
using WebFeedReader.Factories;

namespace WebFeedReader.Api
{
    public sealed class FeedSourceSyncService : IFeedSourceSyncService
    {
        private readonly IApiClient apiClient;
        private readonly IFeedSourceRepository repository;

        public FeedSourceSyncService(IApiClient apiClient, IFeedSourceRepository repository)
        {
            this.apiClient = apiClient;
            this.repository = repository;
        }

        public async Task SyncAsync(DateTime since)
        {
            var json = await apiClient.GetSourcesAsync(since);
            var sources = FeedSourceFactory.FromJson(json);

            foreach (var source in sources)
            {
                await repository.UpsertAsync(source);
            }
        }
    }
}