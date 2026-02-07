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
    }

    public MainWindowViewModel(
        AppSettings appSettings,
        IApiClient apiClient,
        IFeedSourceRepository feedSourceRepository,
        IFeedSourceSyncService feedSourceSyncService,
        IFeedSyncService feedSyncService,
        FeedListViewModel feedListViewModel)
    {
        this.appSettings = appSettings;
        this.apiClient = apiClient;
        this.feedSourceRepository = feedSourceRepository;
        this.feedSourceSyncService = feedSourceSyncService;
        this.feedSyncService = feedSyncService;
        FeedListViewModel = feedListViewModel;

        FeedSourceListViewModel.SelectedItemChanged += async source =>
        {
            await FeedListViewModel.UpdateItemsAsync(source);
        };
    }

    public string Title => appVersionInfo.Title;

    public bool IsLoading { get => isLoading; private set => SetProperty(ref isLoading, value); }

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