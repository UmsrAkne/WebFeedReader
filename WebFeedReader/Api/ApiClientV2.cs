using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using WebFeedReader.Models;
using WebFeedReader.Utils;

namespace WebFeedReader.Api
{
    public sealed class ApiClientV2 : IApiClient
    {
        private readonly HttpClient httpClient = new() { Timeout = TimeSpan.FromSeconds(10), };
        private readonly AppSettings appSettings = AppSettings.Load();

        public async Task<string> GetFeedsAsync(DateTimeOffset since, CancellationToken ct = default)
        {
            var query = HttpUtility.ParseQueryString(string.Empty);
            query["since"] = since.ToString("yyyy-MM-dd HH:mm:ss");

            var url = $"http://{appSettings.ServerUrlWithPort}/feeds?{query}";

            var request = new HttpRequestMessage(HttpMethod.Get, url);
            var response = await httpClient.SendAsync(request, ct);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsStringAsync(ct);
        }

        public async Task<string> GetSourcesAsync(DateTimeOffset since, CancellationToken ct = default)
        {
            var query = HttpUtility.ParseQueryString(string.Empty);
            query["since"] = since.ToString("yyyy-MM-dd HH:mm:ss");

            var url = $"http://{appSettings.ServerUrlWithPort}/sources?{query}";

            var request = new HttpRequestMessage(HttpMethod.Get, url);
            var response = await httpClient.SendAsync(request, ct);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsStringAsync(ct);
        }

        public Task CreateSourceAsync(SourceCreateRequest request, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }

        public async Task<IEnumerable<NgWord>> GetNgWordsAsync(int lastVersion)
        {
            var url = $"http://{appSettings.ServerUrlWithPort}/ng_words";
            var dtoList = await httpClient.GetFromJsonAsync<List<NgWordDto>>(url)
                       ?? new List<NgWordDto>();

            return dtoList.Select(d => new NgWord
            {
                Id = d.Id,
                Value = d.Word,
                CreatedAt = d.CreatedAt,
            });
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

        public Task<IEnumerable<ReadFlagHistory>> GetReadFlagHistoriesAsync(DateTime fromDate)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            httpClient.Dispose();
        }
    }
}