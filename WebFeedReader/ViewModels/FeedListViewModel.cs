using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using Prism.Commands;
using Prism.Mvvm;
using Serilog;
using WebFeedReader.Dbs;
using WebFeedReader.Models;
using WebFeedReader.Utils;

namespace WebFeedReader.ViewModels
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class FeedListViewModel : BindableBase
    {
        private const int PageSize = 200;

        private readonly IFeedItemRepository repository;
        private readonly NgWordService ngWordService;
        private readonly List<FeedItem> readItems = new ();
        private ObservableCollection<FeedItem> items = new ();
        private FeedItem selectedItem;
        private int ngFilteredCount;
        private int currentOffset;
        private bool isLoading;
        private bool hasMoreItems = true;
        private FeedSource currentSource;
        private int? startSelectionIndex;

        public FeedListViewModel(IFeedItemRepository repository, NgWordService ngWordService)
        {
            this.repository = repository;
            this.ngWordService = ngWordService;
            FeedSearchOption = new FeedSearchOption
            {
                NgWordCheckVersion = AppSettings.Load().NgWordListVersion,
            };
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

        public FeedSearchOption FeedSearchOption { get; private set; }

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

        public DelegateCommand<FeedItem> MarkRangeAsReadCommand => new ((item) =>
        {
            // チェックボックスが押されたときに実行されるコマンド。
            // １つ目のチェックなら始点を設定
            // ２つ目のチェックなら指定区間を既読に変更し、終点を始点にセットする。
            if (item == null)
            {
                return;
            }

            // 現在のアイテムのインデックスを取得
            var currentIndex = Items.IndexOf(item);
            if (currentIndex < 0)
            {
                return;
            }

            if (startSelectionIndex == null)
            {
                // 1つ目のチェック: 始点のみを設定（この時点では既読化しない）
                startSelectionIndex = currentIndex;
                return;
            }

            // 2つ目以降のチェック: 範囲を既読にする
            var start = Math.Min(startSelectionIndex.Value, currentIndex);
            var end = Math.Max(startSelectionIndex.Value, currentIndex);

            for (var i = start; i <= end && i < Items.Count; i++)
            {
                var target = Items[i];
                if (!target.IsRead)
                {
                    target.IsRead = true;
                }

                // DB 反映用のキューに追加（重複は避ける）
                if (!readItems.Contains(target))
                {
                    readItems.Add(target);
                }
            }

            Items[startSelectionIndex.Value].IsPreviewSelected = false;

            // 終点を新たな始点に設定
            startSelectionIndex = currentIndex;
        });

        public AsyncRelayCommand<FeedItem> ToggleFavoriteCommand => new (async (param) =>
        {
            if (param == null)
            {
                return;
            }

            param.IsFavorite = !param.IsFavorite;
            await repository.MarkAsFavoriteAsync(param.Key, param.IsFavorite);
        });

        public AsyncRelayCommand LoadAsyncCommand => new (async () =>
        {
            if (currentSource == null)
            {
                return;
            }

            // ロードの状態をリセットする
            hasMoreItems = true;
            currentOffset = 0;
            Items.Clear();
            await LoadNextPageAsync(currentSource);
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
            currentSource = source;

            // !hasMoreItems を外すと、スクロール終端で無限ロードが起こるので絶対必須。
            if (isLoading || !hasMoreItems)
            {
                return;
            }

            isLoading = true;

            // UI スレッドで必要な情報だけをスナップショット
            var offset = currentOffset;
            var pageSize = PageSize;
            var searchOption = FeedSearchOption;
            var baseLineNumber = Items.Count != 0 ? Items.Max(i => i.LineNumber) : 0;

            // 重い処理（DB 取得、NG チェック、前処理）はバックグラウンドで実行
            var result = await Task.Run(async () =>
            {
                var list = await repository.GetBySourceIdPagedAsync(source.Id, offset, pageSize, searchOption);
                if (list.Count == 0)
                {
                    return (Visible: new List<FeedItem>(), TotalCount: 0, NgFiltered: 0);
                }

                // NG チェックと反映
                var checkResults = await ngWordService.Check(list);
                await repository.ApplyNgCheckResultsAsync(checkResults);
                foreach (var r in checkResults)
                {
                    list.First(i => i.Id == r.FeedId).IsNg = r.IsNg;
                }

                // 非表示対象を除外し、行番号を事前に設定
                var visible = list.Where(f => !f.IsNg).ToList();
                for (var i = 0; i < visible.Count; i++)
                {
                    visible[i].LineNumber = baseLineNumber + i;
                }

                var ngCount = list.Count(f => f.IsNg);
                return (Visible: visible, TotalCount: list.Count, NgFiltered: ngCount);
            });

            // UI スレッドに戻って Items に追加と各種カウンタ更新
            if (result.TotalCount == 0)
            {
                hasMoreItems = false;
                isLoading = false;
                return;
            }

            foreach (var item in result.Visible)
            {
                Items.Add(item);
            }

            NgFilteredCount += result.NgFiltered;
            currentOffset += result.TotalCount;

            Log.Information(
                "Loaded {@PageInfo}",
                new
                {
                    VisibleCount = Items.Count,
                    NgFilteredCount,
                    Offset = currentOffset,
                });

            await FlushReadItemsAsync();

            isLoading = false;
        }

        public async Task FlushReadItemsAsync()
        {
            try
            {
                if (repository == null)
                {
                    // Design-time or default constructor: nothing to flush
                    readItems.Clear();
                    return;
                }

                if (readItems.Count == 0)
                {
                    return;
                }

                await repository.MarkAsReadAsync(readItems.Select(i => i.Key));
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to flush read items to database");
            }
            finally
            {
                readItems.Clear();
            }
        }
    }
}