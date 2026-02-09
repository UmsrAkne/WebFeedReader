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

            var startInfo = new ProcessStartInfo
            {
                FileName = "ssh",
                Arguments = $"-N -L 8000:127.0.0.1:8000 {appSettings.SshUserName}",
                UseShellExecute = false,
                CreateNoWindow = true,
            };

            sshProcess = Process.Start(startInfo)
                         ?? throw new InvalidOperationException("Failed to start ssh process");
        }

        private async Task<string> GetAsync(string url, CancellationToken ct)
        {
            using var response = await httpClient.GetAsync(url, ct);

            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsStringAsync(ct);
        }
    }
}