using System;
using CommunityToolkit.Mvvm.Input;
using Prism.Mvvm;
using Serilog;
using WebFeedReader.Api;

namespace WebFeedReader.ViewModels
{
    public class FeedSourceCreatePageViewModel : BindableBase
    {
        private readonly FeedSourceService feedSourceService;

        private string pageTitle = string.Empty;

        private string url = string.Empty;

        // 利用時に想定される最低値を初期値に設定。
        private int checkInterval = 60;

        public FeedSourceCreatePageViewModel(FeedSourceService feedSourceService)
        {
            this.feedSourceService = feedSourceService;
        }

        public string PageTitle
        {
            get => pageTitle;
            set
            {
                if (SetProperty(ref pageTitle, value))
                {
                    RaisePropertyChanged(nameof(CanCreateSource));
                }
            }
        }

        public string Url
        {
            get => url;
            set
            {
                if (SetProperty(ref url, value))
                {
                    RaisePropertyChanged(nameof(CanCreateSource));
                }
            }
        }

        /// <summary>
        /// ページの更新間隔。 分単位、最低 60 分の入力を想定。
        /// </summary>
        public int CheckInterval
        {
            get => checkInterval;
            set
            {
                if (SetProperty(ref checkInterval, value))
                {
                    RaisePropertyChanged(nameof(CanCreateSource));
                }
            }
        }

        public bool CanCreateSource =>
            !string.IsNullOrWhiteSpace(PageTitle) && !string.IsNullOrWhiteSpace(Url) && CheckInterval >= 60;

        public AsyncRelayCommand CreateFeedSourceRequestCommand => new (async () =>
        {
            try
            {
                await feedSourceService.AddSourceAsync(PageTitle, Url, CheckInterval);

                // 成功したら入力をクリアするなどの処理
                PageTitle = string.Empty;
                Url = string.Empty;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to add source");
            }
        });
    }
}