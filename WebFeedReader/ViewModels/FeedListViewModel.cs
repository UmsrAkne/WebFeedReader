using System.Collections.ObjectModel;
using Prism.Mvvm;
using WebFeedReader.Dbs;
using WebFeedReader.Models;

namespace WebFeedReader.ViewModels
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class FeedListViewModel : BindableBase
    {
        private readonly IFeedItemRepository repository;
        private ObservableCollection<FeedItem> items = new ();
        private FeedItem selectedItem;

        public FeedListViewModel(IFeedItemRepository repository)
        {
            this.repository = repository;
        }

        public ObservableCollection<FeedItem> Items { get => items; set => SetProperty(ref items, value); }

        public FeedItem SelectedItem { get => selectedItem; set => SetProperty(ref selectedItem, value); }

        public void UpdateItems(FeedSource source)
        {
            System.Console.WriteLine($"UpdateItems: {source.Name}");
        }
    }
}