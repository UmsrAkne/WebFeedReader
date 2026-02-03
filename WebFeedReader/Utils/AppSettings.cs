namespace WebFeedReader.Utils
{
    using System;
    using System.IO;
    using System.Text.Json;
    using System.Text.Json.Serialization;

    public sealed class AppSettings
    {
        // ReSharper disable once ArrangeModifiersOrder
        public static readonly DateTime InitialFeedsUpdate = new(2000, 1, 1);

        public string ApiBaseUrl { get; set; } = "http://localhost:8000";

        public int FetchIntervalMinutes { get; set; } = 60;

        public bool EnableDebugLog { get; set; }

        public string SshUserName { get; set; }

        public DateTime LastFeedsUpdate { get; set; } = InitialFeedsUpdate;

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