using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Prism.Mvvm;
using WebFeedReader.Api;
using WebFeedReader.Factories;
using WebFeedReader.Models;
using WebFeedReader.Utils;

namespace WebFeedReader.ViewModels;

// ReSharper disable once ClassNeverInstantiated.Global
public class MainWindowViewModel : BindableBase, IDisposable
{
    private readonly AppVersionInfo appVersionInfo = new ();
    private readonly AppSettings appSettings;
    private readonly IApiClient apiClient;
    private bool isLoading;

    public MainWindowViewModel()
    {
        var json = new DummyApiClient().GetFeedsAsync(DateTime.Now);
        FeedListViewModel.Items.AddRange(FeedItemFactory.FromJson(json.Result, string.Empty));
    }

    public MainWindowViewModel(AppSettings appSettings, IApiClient apiClient)
    {
        this.appSettings = appSettings;
        this.apiClient = apiClient;
    }

    public string Title => appVersionInfo.Title;

    public bool IsLoading { get => isLoading; private set => SetProperty(ref isLoading, value); }

    public ObservableCollection<FeedSource> FeedSources { get; set; }

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
            var sources = FeedSourceFactory.FromJson(sourceJson);

            FeedListViewModel.Items.AddRange(feeds);
            FeedSources = new ObservableCollection<FeedSource>(sources);

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