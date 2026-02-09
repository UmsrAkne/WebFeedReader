using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Prism.Commands;
using Prism.Mvvm;
using Serilog;
using WebFeedReader.Dbs;
using WebFeedReader.Models;

namespace WebFeedReader.ViewModels
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class FeedListViewModel : BindableBase
    {
        private const int PageSize = 100;

        private readonly IFeedItemRepository repository;
        private readonly NgWordService ngWordService;
        private readonly List<FeedItem> readItems = new ();
        private ObservableCollection<FeedItem> items = new ();
        private FeedItem selectedItem;
        private int ngFilteredCount;
        private int currentOffset;
        private bool isLoading;
        private bool hasMoreItems = true;

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

        public async Task OnSourceSelectedAsync(FeedSource source)
        {
            currentOffset = 0;
            hasMoreItems = true;
            Items.Clear();

            await LoadNextPageAsync(source);
        }

        public async Task LoadNextPageAsync(FeedSource source)
        {
            if (isLoading || !hasMoreItems)
            {
                return;
            }

            isLoading = true;

            var list =
                await repository.GetBySourceIdPagedAsync(source.Id, currentOffset, PageSize);

            if (list.Count == 0)
            {
                hasMoreItems = false;
                isLoading = false;
                return;
            }

            // NG チェック
            var checkResults = await ngWordService.Check(list);
            await repository.ApplyNgCheckResultsAsync(checkResults);

            foreach (var r in checkResults)
            {
                list.First(i => i.Id == r.FeedId).IsNg = r.IsNg;
            }

            var visibleItems = list.Where(f => !f.IsNg);

            foreach (var item in visibleItems)
            {
                Items.Add(item);
            }

            NgFilteredCount += list.Count(f => f.IsNg);

            currentOffset += list.Count;
            Log.Information(
                "Loaded {@PageInfo}",
                new
                {
                    VisibleCount = Items.Count,
                    NgFilteredCount,
                    Offset = currentOffset,
                });

            await repository.MarkAsReadAsync(readItems.Select(i => i.Key));
            readItems.Clear();

            isLoading = false;
        }
    }
}