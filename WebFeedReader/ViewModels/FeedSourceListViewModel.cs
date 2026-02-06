using System;
using System.Collections.ObjectModel;
using Prism.Mvvm;
using WebFeedReader.Models;

namespace WebFeedReader.ViewModels
{
    public class FeedSourceListViewModel : BindableBase
    {
        private ObservableCollection<FeedSource> items = new ();

        private FeedSource selectedItem;

        public event Action<FeedSource> SelectedItemChanged;

        public ObservableCollection<FeedSource> Items { get => items; set => SetProperty(ref items, value); }

        public FeedSource SelectedItem
        {
            get => selectedItem;
            set
            {
                if (SetProperty(ref selectedItem, value))
                {
                    SelectedItemChanged?.Invoke(value);
                }
            }
        }
    }
}