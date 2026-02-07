using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Prism.Commands;
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
        private int ngFilteredCount;

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

        public int NgFilteredCount { get => ngFilteredCount; set => SetProperty(ref ngFilteredCount, value); }

        public DelegateCommand<string> OpenUrlCommand => new (url =>
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                return;
            }

            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = url,
                    UseShellExecute = true,
                };

                Process.Start(psi);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
        });

        public DelegateCommand<string> CopyToClipboardCommand => new (param =>
        {
            if (string.IsNullOrWhiteSpace(param))
            {
                return;
            }

            System.Windows.Clipboard.SetText(param);
        });

        public async Task UpdateItemsAsync(FeedSource source)
        {
            var list = await repository.GetBySourceIdAsync(source.Id);

            var checkResults = await ngWordService.Check(list);
            await repository.ApplyNgCheckResultsAsync(checkResults);

            foreach (var r in checkResults)
            {
                list.First(i => i.Id == r.FeedId).IsNg = r.IsNg;
            }

            NgFilteredCount = list.Count(f => f.IsNg);

            Items = new ObservableCollection<FeedItem>(list.Where(f => !f.IsNg));

            await repository.MarkAsReadAsync(readItems.Select(i => i.Key));
            readItems.Clear();

            System.Console.WriteLine($"UpdateItemsAsync: {source.Name}");
        }
    }
}