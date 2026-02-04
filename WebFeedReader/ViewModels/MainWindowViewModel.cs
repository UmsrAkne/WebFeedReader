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
        IFeedSourceSyncService feedSourceSyncService)
    {
        this.appSettings = appSettings;
        this.apiClient = apiClient;
        this.ngWordService = ngWordService;
        this.feedSourceRepository = feedSourceRepository;
        this.feedSourceSyncService = feedSourceSyncService;
    }

    public string Title => appVersionInfo.Title;

    public bool IsLoading { get => isLoading; private set => SetProperty(ref isLoading, value); }

    public int NgFilteredCount { get => ngFilteredCount; private set => SetProperty(ref ngFilteredCount, value); }

    public FeedSourceListViewModel FeedSourceListViewModel { get; set; } = new ();

    public FeedListViewModel FeedListViewModel { get; private set; } = new ();

    public async Task InitializeAsync()
    {
        IsLoading = true;

        try
        {
            var since = appSettings.LastFeedsUpdate;

            var feedJson = await apiClient.GetFeedsAsync(since);

            var feeds = FeedItemFactory.FromJson(feedJson, string.Empty);
            var filtered = await ngWordService.FilterNewFeedsAsync(feeds);

            // Update NG filtered count for status bar
            NgFilteredCount = feeds.Count - filtered.Count;

            await feedSourceSyncService.SyncAsync(since);
            var sources = await feedSourceRepository.GetAllAsync();
            FeedSourceListViewModel.Items.AddRange(sources);

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