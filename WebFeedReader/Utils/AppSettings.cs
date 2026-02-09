namespace WebFeedReader.Utils
{
    using System;
    using System.IO;
    using System.Text.Json;
    using System.Text.Json.Serialization;

    public sealed class AppSettings
    {
        // ReSharper disable once ArrangeModifiersOrder
        public static readonly DateTimeOffset InitialFeedsUpdate
            = new (2000, 1, 1, 0, 0, 0, TimeSpan.Zero);

        public string ApiBaseUrl { get; set; } = "http://localhost:8000";

        public int FetchIntervalMinutes { get; set; } = 60;

        public bool EnableDebugLog { get; set; }

        public string SshUserName { get; set; }

        public DateTimeOffset LastFeedsUpdate { get; set; } = InitialFeedsUpdate;

        public int NgWordListVersion { get; set; }

        [JsonIgnore]
        private static string ConfigPath =>
            Path.Combine(AppContext.BaseDirectory, "app_settings.json");

        public static AppSettings Load()
        {
            if (!File.Exists(ConfigPath))
            {
                var settings = new AppSettings();
                settings.Save();
                return settings;
            }

            var json = File.ReadAllText(ConfigPath);
            return JsonSerializer.Deserialize<AppSettings>(json)
                   ?? new AppSettings();
        }

        public void Save()
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
            };

            var json = JsonSerializer.Serialize(this, options);
            File.WriteAllText(ConfigPath, json);
        }
    }
}