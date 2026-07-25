using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Serilog;
using WebFeedReader.Models;

namespace WebFeedReader.Api
{
    // ダミーの API クライアント。テストで想定している JSON 形式を返す。
    // ReSharper disable once ClassNeverInstantiated.Global
    public sealed class DummyApiClient : IApiClient
    {
        public Task<string> GetFeedsAsync(DateTimeOffset since, CancellationToken ct = default)
        {
            var items = Enumerable.Range(1, 3000)
                .Select(i => new
                {
                    id = i,
                    title = $"サンプル記事 {i:D3}：テスト用フィードデータ",
                    link = $"https://example.com/articles/sample-{i}?source=rss",
                    published = since.AddMinutes(i * 3).ToString("yyyy-MM-ddTHH:mm:ss"),
                    summary = $"{i} summary summary summary summary summary summary summary summary summary summary summary ",
                    source_id = (i % 5) + 1,
                });

            var json = System.Text.Json.JsonSerializer.Serialize(
                items,
                new System.Text.Json.JsonSerializerOptions
                {
                    WriteIndented = true,
                });

            return Task.FromResult(json);
        }

        public Task<string> GetSourcesAsync(DateTimeOffset since, CancellationToken ct = default)
        {
            var sources = Enumerable.Range(1, 10)
                .Select(i => new
                {
                    id = i,
                    name = $"Example Site {i}",
                    url = $"https://example{i}.com/rss",
                    enabled = i % 2 == 0,
                    check_interval_minutes = 30 + (i * 10),
                    updated_at = since.AddHours(-i).ToString("yyyy-MM-ddTHH:mm:ss"),
                    created_at = since.AddDays(-30 - i).ToString("yyyy-MM-ddTHH:mm:ss"),
                });

            var json = System.Text.Json.JsonSerializer.Serialize(
                sources,
                new System.Text.Json.JsonSerializerOptions
                {
                    WriteIndented = true,
                });

            return Task.FromResult(json);
        }

        public async Task CreateSourceAsync(SourceCreateRequest request, CancellationToken ct = default)
        {
            Log.Information("Create Source Async (DummyApiClient). {@requestINfo}", new { request.Name, request.Url, });
            await Task.Delay(2000, ct);
        }

        public Task<IEnumerable<NgWord>> GetNgWordsAsync(int lastVersion)
        {
            Log.Information("Get NgWords Async (DummyApiClient).");
            Log.Information("  - lastVersion: {lastVersion}", lastVersion);
            IEnumerable<NgWord> list = new List<NgWord>();
            return Task.FromResult(list);
        }

        public Task PostReadStatusAsync(IEnumerable<string> feedItemKeys)
        {
            Log.Information("Post Read Status Async (DummyApiClient).");
            Log.Information("  - feedItemKeys: {feedItemKeys}", feedItemKeys);
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            // 何もしない（外部リソース未使用）
        }
    }
}