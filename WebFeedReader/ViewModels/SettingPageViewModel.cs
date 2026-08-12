using Prism.Commands;
using Prism.Mvvm;
using WebFeedReader.Utils;

namespace WebFeedReader.ViewModels
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class SettingPageViewModel : BindableBase
    {
        private readonly AppSettings appSettings;

        private int fetchIntervalMinutes;
        private bool enableDebugLog;
        private string serverUrl;

        public SettingPageViewModel(AppSettings appSettings)
        {
            this.appSettings = appSettings;

            // Initialize editable properties from app settings
            FetchIntervalMinutes = appSettings?.FetchIntervalMinutes ?? 0;
            EnableDebugLog = appSettings?.EnableDebugLog ?? false;

            SaveCommand = new DelegateCommand(Save, CanSave)
                .ObservesProperty(() => FetchIntervalMinutes);
        }

        public string ServerUrl { get => serverUrl; set => SetProperty(ref serverUrl, value); }

        public int FetchIntervalMinutes
        {
            get => fetchIntervalMinutes;
            set => SetProperty(ref fetchIntervalMinutes, value < 0 ? 0 : value);
        }

        public bool EnableDebugLog { get => enableDebugLog; set => SetProperty(ref enableDebugLog, value); }

        // View-only properties
        public string LastFeedsUpdate => (appSettings?.LastFeedsUpdate).ToString();

        public int NgWordListVersion => appSettings?.NgWordListVersion ?? 0;

        public DelegateCommand SaveCommand { get; }

        private bool CanSave()
        {
            // Minimal validation: Base URL and SSH user can be empty, but allow save to always
            // Prevent saving when no AppSettings available (design-time)
            return appSettings != null;
        }

        private void Save()
        {
            if (appSettings == null)
            {
                return;
            }

            appSettings.FetchIntervalMinutes = FetchIntervalMinutes;
            appSettings.EnableDebugLog = EnableDebugLog;
            appSettings.ServerUrlWithPort = ServerUrl?.Trim();
            appSettings.Save();

            // Notify read-only fields if their display could change (not needed here but safe)
            RaisePropertyChanged(nameof(LastFeedsUpdate));
            RaisePropertyChanged(nameof(NgWordListVersion));
        }
    }
}