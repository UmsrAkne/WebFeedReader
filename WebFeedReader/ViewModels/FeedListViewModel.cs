using System.Collections.ObjectModel;
using Prism.Mvvm;
using WebFeedReader.Models;

namespace WebFeedReader.ViewModels
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class FeedListViewModel : BindableBase
    {
        private ObservableCollection<FeedItem> items = new ();
        private FeedItem selectedItem;

        public ObservableCollection<FeedItem> Items { get => items; set => SetProperty(ref items, value); }

        public FeedItem SelectedItem { get => selectedItem; set => SetProperty(ref selectedItem, value); }
    }
}