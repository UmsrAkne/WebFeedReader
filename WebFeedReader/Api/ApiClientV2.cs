using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using WebFeedReader.Models;
using WebFeedReader.Utils;

namespace WebFeedReader.Api
{
    public sealed class ApiClientV2 : IApiClient
    {
        private readonly HttpClient httpClient;
        private readonly AppSettings appSettings;

        public ApiClientV2(AppSettings appSettings)
        {
            httpClient = new HttpClient
            {
                Timeout = TimeSpan.FromSeconds(10),
            };

            this.appSettings = appSettings;
        }

        public Task<string> GetFeedsAsync(DateTimeOffset since, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }

        public Task<string> GetSourcesAsync(DateTimeOffset since, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }

        public Task CreateSourceAsync(SourceCreateRequest request, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<NgWord>> GetNgWordsAsync(int lastVersion)
        {
            throw new NotImplementedException();
        }

        public async Task PostReadStatusAsync(IEnumerable<int> feedItemIds)
        {
            var url = $"http://{appSettings.ServerUrlWithPort}/feeds/read";
            var payload = new { ids = feedItemIds, };

            // シリアライズ
            var response = await httpClient.PostAsJsonAsync(url, payload);

            // レスポンスのエラーチェック
            response.EnsureSuccessStatusCode();
        }

        public void Dispose()
        {
            httpClient.Dispose();
        }
    }
}