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
    private bool isLoading;

    public MainWindowViewModel()
    {
        var feedsJson = new DummyApiClient().GetFeedsAsync(DateTime.Now);
        FeedListViewModel.Items.AddRange(FeedItemFactory.FromJson(feedsJson.Result, string.Empty));

        var sourcesJson = new DummyApiClient().GetSourcesAsync(DateTime.Now);
        FeedSourceListViewModel.Items.AddRange(FeedSourceFactory.FromJson(sourcesJson.Result));

        FeedListViewModel.SelectedItem = FeedListViewModel.Items[0];
    }

    public MainWindowViewModel(AppSettings appSettings, NgWordService ngWordService, IApiClient apiClient)
    {
        this.appSettings = appSettings;
        this.apiClient = apiClient;
        this.ngWordService = ngWordService;
    }

    public string Title => appVersionInfo.Title;

    public bool IsLoading { get => isLoading; private set => SetProperty(ref isLoading, value); }

    public FeedSourceListViewModel FeedSourceListViewModel { get; set; } = new ();

    public FeedListViewModel FeedListViewModel { get; private set; } = new ();

    public async Task InitializeAsync()
    {
        IsLoading = true;

        try
        {
            var since = appSettings.LastFeedsUpdate;

            var feedJson = await apiClient.GetFeedsAsync(since);
            var sourceJson = await apiClient.GetSourcesAsync(since);

            var feeds = FeedItemFactory.FromJson(feedJson, string.Empty);
            var filtered = await ngWordService.FilterNewFeedsAsync(feeds);

            var sources = FeedSourceFactory.FromJson(sourceJson);

            FeedListViewModel.Items.AddRange(filtered);
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