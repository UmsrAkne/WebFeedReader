using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Prism.Mvvm;
using WebFeedReader.Dbs;
using WebFeedReader.Models;

namespace WebFeedReader.ViewModels
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class FeedListViewModel : BindableBase
    {
        private readonly IFeedItemRepository repository;
        private readonly NgWordService ngWordService;
        private readonly List<FeedItem> readItems = new ();
        private ObservableCollection<FeedItem> items = new ();
        private FeedItem selectedItem;

        public FeedListViewModel(IFeedItemRepository repository, NgWordService ngWordService)
        {
            this.repository = repository;
            this.ngWordService = ngWordService;
        }

        public ObservableCollection<FeedItem> Items { get => items; private set => SetProperty(ref items, value); }

        public FeedItem SelectedItem
        {
            get => selectedItem;
            set
            {
                if (value != null)
                {
                    value.IsRead = true;
                    readItems.Add(value);
                }

                SetProperty(ref selectedItem, value);
            }
        }

        public async Task UpdateItemsAsync(FeedSource source)
        {
            var list = await repository.GetBySourceIdAsync(source.Id);

            var checkResults = await ngWordService.Check(list);
            await repository.ApplyNgCheckResultsAsync(checkResults);

            Items = new ObservableCollection<FeedItem>(list);

            await repository.MarkAsReadAsync(readItems.Select(i => i.Key));
            readItems.Clear();

            System.Console.WriteLine($"UpdateItemsAsync: {source.Name}");
        }
    }
}