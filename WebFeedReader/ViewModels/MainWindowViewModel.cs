using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using Prism.Mvvm;
using Serilog;
using WebFeedReader.Api;
using WebFeedReader.Dbs;
using WebFeedReader.Factories;
using WebFeedReader.Utils;

namespace WebFeedReader.ViewModels;

// ReSharper disable once ClassNeverInstantiated.Global
public class MainWindowViewModel : BindableBase, IScrollResettable
{
    private readonly AppVersionInfo appVersionInfo = new ();
    private readonly AppSettings appSettings;
    private readonly IFeedSourceSyncService feedSourceSyncService;
    private readonly IFeedSourceRepository feedSourceRepository;
    private readonly IFeedSyncService feedSyncService;
    private bool isLoading;

    public MainWindowViewModel()
    {
        FeedListViewModel = new FeedListViewModel(null, null);

        var feedsJson = new DummyApiClient().GetFeedsAsync(DateTime.Now);
        FeedListViewModel.Items.AddRange(FeedItemFactory.FromJson(feedsJson.Result, string.Empty));

        var sourcesJson = new DummyApiClient().GetSourcesAsync(DateTime.Now);
        FeedSourceListViewModel.Items.AddRange(FeedSourceFactory.FromJson(sourcesJson.Result));

        FeedListViewModel.SelectedItem = FeedListViewModel.Items[0];

        SettingPageViewModel = new SettingPageViewModel(null);
    }

    public MainWindowViewModel(
        AppSettings appSettings,
        IFeedSourceRepository feedSourceRepository,
        IFeedSourceSyncService feedSourceSyncService,
        IFeedSyncService feedSyncService,
        FeedListViewModel feedListViewModel,
        SettingPageViewModel settingPageViewModel)
    {
        this.appSettings = appSettings;
        this.feedSourceRepository = feedSourceRepository;
        this.feedSourceSyncService = feedSourceSyncService;
        this.feedSyncService = feedSyncService;
        FeedListViewModel = feedListViewModel;
        SettingPageViewModel = settingPageViewModel;

        FeedSourceListViewModel.SelectedItemChanged += async source =>
        {
            RequestScrollReset?.Invoke();
            await FeedListViewModel.OnSourceSelectedAsync(source);
        };
    }

    public event Action RequestScrollReset;

    public string Title => appVersionInfo.Title;

    public bool IsLoading { get => isLoading; private set => SetProperty(ref isLoading, value); }

    public FeedSourceListViewModel FeedSourceListViewModel { get; set; } = new ();

    public FeedListViewModel FeedListViewModel { get; private set; }

    public SettingPageViewModel SettingPageViewModel { get; }

    public AsyncRelayCommand ReloadAsyncCommand => new (async () => await ReloadAsync());

    public async Task InitializeAsync()
    {
        IsLoading = true;
        try
        {
            await SyncFeedsAsync(appSettings.LastFeedsUpdate);

            var sources = await feedSourceRepository.GetAllAsync();
            FeedSourceListViewModel.Items.AddRange(sources);
        }
        catch(Exception ex)
        {
            Log.Error(ex, "Failed to initial load feeds");
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task ReloadAsync()
    {
        IsLoading = true;
        try
        {
            await SyncFeedsAsync(appSettings.LastFeedsUpdate);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to reload feeds");
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task SyncFeedsAsync(DateTimeOffset since)
    {
        await feedSourceSyncService.SyncAsync(since);
        await feedSyncService.SyncAsync(since);

        appSettings.LastFeedsUpdate = DateTimeOffset.UtcNow;
        appSettings.Save();
    }
}