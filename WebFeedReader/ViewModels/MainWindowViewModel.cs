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
    private readonly ApiClient apiClient;
    private bool isLoading;

    public MainWindowViewModel()
    {
    }

    public MainWindowViewModel(AppSettings appSettings)
    {
        this.appSettings = appSettings;
        apiClient = new ApiClient(appSettings);
    }

    public string Title => appVersionInfo.Title;

    public bool IsLoading { get => isLoading; private set => SetProperty(ref isLoading, value); }

    public ObservableCollection<FeedSource> FeedSources { get; set; }

    public ObservableCollection<FeedItem> FeedItems { get; set; }

    public async Task InitializeAsync()
    {
        IsLoading = true;

        try
        {
            var since = DateTime.Now.AddDays(-1);

            var feedJson = await apiClient.GetFeedsAsync(since);
            var sourceJson = await apiClient.GetSourcesAsync(since);

            var feeds = FeedItemFactory.FromJson(feedJson, string.Empty);
            var sources = FeedSourceFactory.FromJson(sourceJson);

            FeedItems = new ObservableCollection<FeedItem>(feeds);
            FeedSources = new ObservableCollection<FeedSource>(sources);
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