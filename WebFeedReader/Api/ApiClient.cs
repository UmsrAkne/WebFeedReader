using System;
using System.Diagnostics;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using WebFeedReader.Utils;

namespace WebFeedReader.Api
{
    public sealed class ApiClient : IApiClient, IDisposable
    {
        private const string BaseUrl = "http://127.0.0.1:8000";

        private readonly HttpClient httpClient;
        private readonly AppSettings appSettings;
        private Process sshProcess;

        public ApiClient(AppSettings appSettings)
        {
            httpClient = new HttpClient
            {
                Timeout = TimeSpan.FromSeconds(10),
            };

            this.appSettings = appSettings;
        }

        public async Task<string> GetFeedsAsync(DateTimeOffset since, CancellationToken ct = default)
        {
            EnsureSshTunnel();

            var sinceText = FormatDateTime(since);
            var url = $"{BaseUrl}/feeds?since={Uri.EscapeDataString(sinceText)}";

            return await GetAsync(url, ct);
        }

        public async Task<string> GetSourcesAsync(DateTimeOffset since, CancellationToken ct = default)
        {
            EnsureSshTunnel();

            var sinceText = FormatDateTime(since);
            var url = $"{BaseUrl}/sources?since={Uri.EscapeDataString(sinceText)}";

            return await GetAsync(url, ct);
        }

        public void Dispose()
        {
            try
            {
                if (sshProcess is { HasExited: false, })
                {
                    sshProcess.Kill(true);
                    sshProcess.Dispose();
                }
            }
            catch
            {
                // 無視（終了時だし）
            }

            httpClient.Dispose();
        }

        private static string FormatDateTime(DateTimeOffset dt)
        {
            return dt.UtcDateTime.ToString("yyyy-MM-dd HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture);
        }

        private void EnsureSshTunnel()
        {
            if (sshProcess is { HasExited: false, })
            {
                return;
            }

            // SSHの挙動を制御するオプション群
            var options = new[]
            {
                "-N", // コマンドを実行せず、ポート転送のみ行う
                "-L 8000:127.0.0.1:8000",
                "-o ConnectTimeout=5",          // 接続自体のタイムアウト（秒）
                "-o ExitOnForwardFailure=yes",  // ポート転送に失敗（他が使用中など）したら即終了する ★重要
                "-o ServerAliveInterval=15",    // 生存確認パケットを送り、無反応なら切断する
                "-o StrictHostKeyChecking=no",  // ホストキーの確認で止まるのを防ぐ（環境による）
            };

            var startInfo = new ProcessStartInfo
            {
                FileName = "ssh",
                Arguments = $"{string.Join(" ", options)} {appSettings.SshUserName}",
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardError = true,
            };

            sshProcess = Process.Start(startInfo)
                         ?? throw new InvalidOperationException("Failed to start ssh process");
        }

        private async Task<string> GetAsync(string url, CancellationToken ct)
        {
            for (var i = 0; i < 3; i++)
            {
                try
                {
                    using var response = await httpClient.GetAsync(url, ct);
                    response.EnsureSuccessStatusCode();
                    return await response.Content.ReadAsStringAsync(ct);
                }
                catch (HttpRequestException) when (i < 2)
                {
                    // トンネルが開通するのを少し待ってリトライ
                    await Task.Delay(1000, ct);
                    EnsureSshTunnel();
                }
            }

            throw new Exception("SSHトンネル経由での接続に失敗しました。");
        }
    }
}