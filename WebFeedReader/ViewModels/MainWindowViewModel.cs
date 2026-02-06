using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Prism.Mvvm;
using WebFeedReader.Api;
using WebFeedReader.Dbs;
using WebFeedReader.Factories;
using WebFeedReader.Utils;

namespace WebFeedReader.ViewModels;

// ReSharper disable once ClassNeverInstantiated.Global
public class MainWindowViewModel : BindableBase, IDisposable
{
    private readonly AppVersionInfo appVersionInfo = new ();
    private readonly AppSettings appSettings;
    private readonly IApiClient apiClient;
    private readonly NgWordService ngWordService;
    private readonly IFeedSourceSyncService feedSourceSyncService;
    private readonly IFeedSourceRepository feedSourceRepository;
    private readonly IFeedItemRepository feedItemRepository;
    private readonly IFeedSyncService feedSyncService;
    private bool isLoading;
    private int ngFilteredCount;

    public MainWindowViewModel()
    {
        var feedsJson = new DummyApiClient().GetFeedsAsync(DateTime.Now);
        FeedListViewModel.Items.AddRange(FeedItemFactory.FromJson(feedsJson.Result, string.Empty));

        var sourcesJson = new DummyApiClient().GetSourcesAsync(DateTime.Now);
        FeedSourceListViewModel.Items.AddRange(FeedSourceFactory.FromJson(sourcesJson.Result));

        FeedListViewModel.SelectedItem = FeedListViewModel.Items[0];
    }

    public MainWindowViewModel(
        AppSettings appSettings,
        NgWordService ngWordService,
        IApiClient apiClient,
        IFeedSourceRepository feedSourceRepository,
        IFeedSourceSyncService feedSourceSyncService,
        IFeedItemRepository feedItemRepository,
        IFeedSyncService feedSyncService,
        FeedListViewModel feedListViewModel)
    {
        this.appSettings = appSettings;
        this.apiClient = apiClient;
        this.ngWordService = ngWordService;
        this.feedSourceRepository = feedSourceRepository;
        this.feedSourceSyncService = feedSourceSyncService;
        this.feedItemRepository = feedItemRepository;
        this.feedSyncService = feedSyncService;
        FeedListViewModel = feedListViewModel;

        FeedSourceListViewModel.SelectedItemChanged += async source =>
        {
            await FeedListViewModel.UpdateItemsAsync(source);
        };
    }

    public string Title => appVersionInfo.Title;

    public bool IsLoading { get => isLoading; private set => SetProperty(ref isLoading, value); }

    public int NgFilteredCount { get => ngFilteredCount; private set => SetProperty(ref ngFilteredCount, value); }

    public FeedSourceListViewModel FeedSourceListViewModel { get; set; } = new ();

    public FeedListViewModel FeedListViewModel { get; private set; }

    public async Task InitializeAsync()
    {
        IsLoading = true;

        try
        {
            var since = appSettings.LastFeedsUpdate;

            await feedSourceSyncService.SyncAsync(since);
            await feedSyncService.SyncAsync(since);

            var sources = await feedSourceRepository.GetAllAsync();
            FeedSourceListViewModel.Items.AddRange(sources);

            var feeds = await feedItemRepository.GetAllAsync();
            var filtered = await ngWordService.FilterNewFeedsAsync(feeds);

            // Update NG filtered count for status bar
            NgFilteredCount = feeds.Count - filtered.Count;

            FeedListViewModel.Items.AddRange(filtered);

            appSettings.LastFeedsUpdate = DateTime.Now;
            appSettings.Save();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
        finally
        {
            IsLoading = false;
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        apiClient.Dispose();
    }
}