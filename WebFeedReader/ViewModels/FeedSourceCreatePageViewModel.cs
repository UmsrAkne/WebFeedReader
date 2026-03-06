using CommunityToolkit.Mvvm.Input;
using Prism.Mvvm;
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

        public string PageTitle { get => pageTitle; set => SetProperty(ref pageTitle, value); }

        public string Url { get => url; set => SetProperty(ref url, value); }

        /// <summary>
        /// ページの更新間隔。 分単位、最低 60 分の入力を想定。
        /// </summary>
        public int CheckInterval { get => checkInterval; set => SetProperty(ref checkInterval, value); }

        public AsyncRelayCommand CreateFeedSourceRequestCommand => new (async () =>
        {
            await feedSourceService.AddSourceAsync(PageTitle, Url, CheckInterval);
        }, () => !string.IsNullOrWhiteSpace(pageTitle) && string.IsNullOrWhiteSpace(url) && CheckInterval < 60);
    }
}