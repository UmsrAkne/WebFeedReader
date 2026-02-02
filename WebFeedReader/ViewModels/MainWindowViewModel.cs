using System.Threading.Tasks;
using Prism.Mvvm;
using WebFeedReader.Utils;

namespace WebFeedReader.ViewModels;

public class MainWindowViewModel : BindableBase
{
    private readonly AppVersionInfo appVersionInfo = new ();
    private readonly AppSettings appSettings;
    private bool isLoading;

    public MainWindowViewModel()
    {
    }

    public MainWindowViewModel(AppSettings appSettings)
    {
        this.appSettings = appSettings;
    }

    public string Title => appVersionInfo.Title;

    public bool IsLoading { get => isLoading; private set => SetProperty(ref isLoading, value); }

    public async Task InitializeAsync()
    {
        IsLoading = true;

        try
        {
            // ダミー処理
            await Task.CompletedTask;

            // var json = await apiClient.GetFeedsAsync();
            // var items = FeedItemFactory.FromJson(json);
            // FeedItems = new ObservableCollection<FeedItem>(items);
        }
        finally
        {
            IsLoading = false;
        }
    }
}