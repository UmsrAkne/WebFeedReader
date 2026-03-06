using System;
using System.Threading.Tasks;
using Serilog;
using WebFeedReader.Dbs;
using WebFeedReader.Factories;
using WebFeedReader.Utils;

namespace WebFeedReader.Api
{
    public sealed class FeedSourceService : IFeedSourceSyncService
    {
        private readonly IApiClient apiClient;
        private readonly IFeedSourceRepository repository;

        public FeedSourceService(IApiClient apiClient, IFeedSourceRepository repository)
        {
            this.apiClient = apiClient;
            this.repository = repository;
        }

        public async Task SyncAsync(DateTimeOffset since)
        {
            var json = await apiClient.GetSourcesAsync(since);
            json = DateTimeFormatFixer.FixDateTimeFormat(json);
            var sources = FeedSourceFactory.FromJson(json);

            foreach (var source in sources)
            {
                await repository.UpsertAsync(source);
            }
        }

        public async Task AddSourceAsync(string name, string url, int interval)
        {
            var request = new SourceCreateRequest
            {
                Name = name,
                Url = url,
                CheckIntervalMinutes = interval,
            };

            try
            {
                await apiClient.CreateSourceAsync(request);
                Log.Information("Source added successfully: {Url}", url);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to add source: {Url}", url);
                throw; // 必要に応じてカスタム例外にラップ
            }
        }
    }
}