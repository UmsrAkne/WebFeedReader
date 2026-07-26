using System;
using System.Collections.ObjectModel;
using System.Linq;
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
    private readonly IFeedItemRepository feedItemRepository;
    private bool isLoading;
    private AsyncRelayCommand initializeCommand;
    private readonly NgWordService ngWordService;

    public MainWindowViewModel()
    {
        FeedListViewModel = new FeedListViewModel(null, null, null, null);

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
        IFeedItemRepository feedItemRepository,
        FeedListViewModel feedListViewModel,
        NgListPageViewModel ngListPageViewModel,
        FeedSourceCreatePageViewModel feedSourceCreatePageViewModel,
        SettingPageViewModel settingPageViewModel,
        FeedSourceListViewModel feedListVm,
        NgWordService ngWordService)
    {
        this.appSettings = appSettings;
        this.feedSourceRepository = feedSourceRepository;
        this.feedSourceSyncService = feedSourceSyncService;
        this.feedSyncService = feedSyncService;
        this.feedItemRepository = feedItemRepository;
        FeedListViewModel = feedListViewModel;
        NgListPageViewModel = ngListPageViewModel;
        FeedSourceCreatePageViewModel = feedSourceCreatePageViewModel;
        SettingPageViewModel = settingPageViewModel;
        FeedSourceListViewModel = feedListVm;

        this.ngWordService = ngWordService;

        FeedSourceListViewModel.SelectedItemChanged += async source =>
        {
            RequestScrollReset?.Invoke();
            await FeedListViewModel.OnSourceSelectedAsync(source);
        };
    }

    public event Action RequestScrollReset;

    public string Title => appVersionInfo.Title;

    public bool IsLoading { get => isLoading; private set => SetProperty(ref isLoading, value); }

    public FeedSourceListViewModel FeedSourceListViewModel { get; set; }

    public FeedListViewModel FeedListViewModel { get; private set; }

    public NgListPageViewModel NgListPageViewModel { get; }

    public FeedSourceCreatePageViewModel FeedSourceCreatePageViewModel { get; }

    public SettingPageViewModel SettingPageViewModel { get; }

    public AsyncRelayCommand ReloadAsyncCommand => new (async () => await ReloadAsync());

    public AsyncRelayCommand InitializeAsyncCommand =>
        initializeCommand ??= new AsyncRelayCommand(async () =>
        {
            await InitializeAsync();
        });

    private async Task InitializeAsync()
    {
        IsLoading = true;
        try
        {
            var ngWords = await ngWordService.GetAllNgWordsAsync();
            var latestNgWordVersion = 0;
            var ngWordList = ngWords.ToList();
            if (ngWordList.Any())
            {
                latestNgWordVersion = ngWordList.Max(w => w.CreatedAt);
            }

            await ngWordService.SyncNgWordsAsync(latestNgWordVersion);

            await SyncFeedsAsync(appSettings.LastFeedsUpdate);

            var sources = await feedSourceRepository.GetAllAsync();
            FeedSourceListViewModel.Items.AddRange(sources);
            var ngCheckVersion = AppSettings.Load().NgWordListVersion;
            foreach (var feedSource in FeedSourceListViewModel.Items)
            {
                feedSource.UnreadCount = await feedItemRepository.GetRecentUnreadSafeCountAsync(feedSource.Id, ngCheckVersion);
            }
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