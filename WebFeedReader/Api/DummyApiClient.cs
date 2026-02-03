using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace WebFeedReader.Api
{
    // ダミーの API クライアント。テストで想定している JSON 形式を返す。
    // ReSharper disable once ClassNeverInstantiated.Global
    public sealed class DummyApiClient : IApiClient
    {
        public Task<string> GetFeedsAsync(DateTime since, CancellationToken ct = default)
        {
            var items = Enumerable.Range(1, 100)
                .Select(i => new
                {
                    title = $"サンプル記事 {i:D3}：テスト用フィードデータ",
                    link = $"https://example.com/articles/sample-{i}?source=rss",
                    published = since.AddMinutes(i * 3).ToString("yyyy-MM-ddTHH:mm:ss"),
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

        public Task<string> GetSourcesAsync(DateTime since, CancellationToken ct = default)
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

        public void Dispose()
        {
            // 何もしない（外部リソース未使用）
        }
    }
}