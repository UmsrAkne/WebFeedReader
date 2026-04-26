using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
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
        private readonly IFeedItemRepository repository;
        private readonly NgWordService ngWordService;
        private readonly IReadHistoryRepository readHistoryRepository;
        private readonly List<FeedItem> readItems = new ();
        private readonly PaginationStatus paginationStatus = new ();
        private readonly SemaphoreSlim loadSemaphore = new (1, 1);
        private CancellationTokenSource cts;
        private ObservableCollection<FeedItem> items = new ();
        private FeedItem selectedItem;
        private int ngFilteredCount;
        private int? startSelectionIndex;
        private AsyncRelayCommand<string> openUrlAsyncCommand;

        public FeedListViewModel(IFeedItemRepository repository, IReadHistoryRepository readHistoryRepository, NgWordService ngWordService)
        {
            this.repository = repository;
            this.ngWordService = ngWordService;
            this.readHistoryRepository = readHistoryRepository;
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

        public bool IsLoading => paginationStatus.IsLoading;

        public int NgFilteredCount { get => ngFilteredCount; set => SetProperty(ref ngFilteredCount, value); }

        public AsyncRelayCommand<string> OpenUrlAsyncCommand =>
            openUrlAsyncCommand ??= new AsyncRelayCommand<string>(async (url) =>
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

                    var history = new ReadHistory
                    {
                        FeedItemId = SelectedItem.Id,
                        ReadAt = DateTime.Now,
                    };

                    await readHistoryRepository.AddAsync(history);
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

            if (startSelectionIndex == currentIndex)
            {
                // １つ目と同じ場所がチェックされた場合は、セレクションを解除する
                startSelectionIndex = null;
                return;
            }

            // 2つ目以降のチェック: 範囲を既読にする
            var start = Math.Min(startSelectionIndex.Value, currentIndex);
            var end = Math.Max(startSelectionIndex.Value, currentIndex);

            for (var i = start; i <= end && i < Items.Count; i++)
            {
                var target = Items[i];

                // 処理範囲を確定した時点で、すべてのチェックマークを解除
                target.IsPreviewSelected = false;

                if (!target.IsRead)
                {
                    target.IsRead = true;
                }

                // DB 反映用のキューに追加（重複は避ける）
                if (readItems.All(f => f.Key != target.Key))
                {
                    readItems.Add(target);
                }
            }

            // 終点を新たな始点に設定
            startSelectionIndex = currentIndex;

            // 自動設定された始点をユーザーに明示する
            Items[currentIndex].IsPreviewSelected = true;
        });

        public DelegateCommand<FeedItem> PreviewRangeCommand => new((hoveredItem) =>
        {
            // 始点がなければ何もしない
            if (startSelectionIndex == null)
            {
                return;
            }

            var currentIndex = Items.IndexOf(hoveredItem);
            var start = Math.Min(startSelectionIndex.Value, currentIndex);
            var end = Math.Max(startSelectionIndex.Value, currentIndex);

            // 全アイテムのプレビュー状態を更新
            // ※パフォーマンスが気になるなら「前回の範囲」だけを操作する
            for (var i = 0; i < Items.Count; i++)
            {
                Items[i].IsPreviewSelected = i >= start && i <= end;
            }
        });

        public DelegateCommand CancelSelectionCommand => new (() =>
        {
            if (startSelectionIndex != null)
            {
                startSelectionIndex = null;
                foreach(var item in Items.Where(i => i.IsPreviewSelected))
                {
                    item.IsPreviewSelected = false;
                }
            }
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

        public AsyncRelayCommand<FeedItem> MarkAsUnreadCommand => new (async (param) =>
        {
            if (param == null)
            {
                return;
            }

            param.IsRead = false;
            readItems.Remove(param);

            await repository.MarkAsUnreadAsync(param.Key);
        });

        public AsyncRelayCommand LoadAsyncCommand => new (async () =>
        {
            if (paginationStatus.CurrentSource == null)
            {
                return;
            }

            cts?.Cancel();
            cts = new CancellationTokenSource();

            // ロードの状態をリセットする
            paginationStatus.HasMoreItems = true;
            paginationStatus.CurrentOffset = 0;
            Items.Clear();
            startSelectionIndex = null;
            await LoadNextPageAsync(paginationStatus.CurrentSource);
        });

        public async Task OnSourceSelectedAsync(FeedSource source)
        {
            cts?.Cancel();
            cts = new CancellationTokenSource();

            paginationStatus.CurrentOffset = 0;
            paginationStatus.HasMoreItems = true;
            Items.Clear();
            startSelectionIndex = null;

            await FlushReadItemsAsync();
            await LoadNextPageAsync(source);
        }

        public async Task LoadNextPageAsync(FeedSource source)
        {
            if (cts == null || cts.IsCancellationRequested)
            {
                cts = new CancellationTokenSource();
            }

            var token = cts.Token;

            await loadSemaphore.WaitAsync(token);
            try
            {
                paginationStatus.CurrentSource = source;

                // !hasMoreItems を外すと、スクロール終端で無限ロードが起こるので絶対必須。
                if (paginationStatus.IsLoading || !paginationStatus.HasMoreItems)
                {
                    return;
                }

                paginationStatus.IsLoading = true;

                // UI スレッドで必要な情報だけをスナップショット
                var offset = paginationStatus.CurrentOffset;
                var pageSize = paginationStatus.PageSize;
                var searchOption = FeedSearchOption;
                var baseLineNumber = Items.Count != 0 ? Items.Max(i => i.LineNumber) : 0;

                // 重い処理（DB 取得、NG チェック、前処理）はバックグラウンドで実行
                var result = await Task.Run(
                    async () =>
                    {
                        var list = await repository.GetBySourceIdPagedAsync(source.Id, offset, pageSize, searchOption);
                        if (list.Count == 0)
                        {
                            return (Visible: new List<FeedItem>(), TotalCount: 0, NgFiltered: 0);
                        }

                        token.ThrowIfCancellationRequested();

                        // NG チェックと反映
                        var checkResults = await ngWordService.Check(list);
                        await repository.ApplyNgCheckResultsAsync(checkResults);
                        foreach (var r in checkResults)
                        {
                            list.First(i => i.Id == r.FeedId).IsNg = r.IsNg;
                        }

                        token.ThrowIfCancellationRequested();

                        // 非表示対象を除外し、行番号を事前に設定
                        var visible = list.Where(f => !f.IsNg).ToList();
                        for (var i = 0; i < visible.Count; i++)
                        {
                            visible[i].LineNumber = baseLineNumber + i;
                        }

                        var ngCount = list.Count(f => f.IsNg);
                        return (Visible: visible, TotalCount: list.Count, NgFiltered: ngCount);
                    },
                    token);

                // UI スレッドに戻って Items に追加と各種カウンタ更新
                if (result.TotalCount == 0)
                {
                    paginationStatus.HasMoreItems = false;
                    paginationStatus.IsLoading = false;
                    return;
                }

                var chunkSize = 1; // 1回に追加する量。徐々に増やす。
                var list = result.Visible.ToList();

                for (var i = 0; i < list.Count; i += chunkSize)
                {
                    token.ThrowIfCancellationRequested();
                    chunkSize++; // 処理一回毎に追加量を増やす。

                    var itemsToAdd = list.Skip(i).Take(chunkSize).ToList();

                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        foreach (var item in itemsToAdd)
                        {
                            // キャンセルされていたら追加を中断
                            if (token.IsCancellationRequested)
                            {
                                break;
                            }

                            Items.Add(item);
                        }
                    });

                    const int batchThreshold = 40; // 表示されるのは 30 件程度が最大だが、多めにとっておく
                    if (i >= batchThreshold)
                    {
                        // 画面外の要素の追加までユーザーに見せる必要はないので一括追加
                        var remainingItems = list.Skip(i + chunkSize).ToList();
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            foreach (var item in remainingItems)
                            {
                                if (token.IsCancellationRequested)
                                {
                                    break;
                                }

                                Items.Add(item);
                            }
                        });
                        break;
                    }

                    await Task.Delay(50, token); // 塊ごとに少し長めのウェイト
                }

                token.ThrowIfCancellationRequested();

                NgFilteredCount += result.NgFiltered;
                paginationStatus.CurrentOffset += result.TotalCount;

                Log.Information(
                    "Loaded {@PageInfo}",
                    new
                    {
                        VisibleCount = Items.Count,
                        NgFilteredCount,
                        Offset = paginationStatus.CurrentOffset,
                    });

                await FlushReadItemsAsync();
            }
            catch (OperationCanceledException)
            {
                Log.Information("LoadNextPageAsync was cancelled.");
            }
            finally
            {
                paginationStatus.IsLoading = false;
                loadSemaphore.Release();
            }
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

                // 競合を避けるため、操作はコピーに対して行う
                List<FeedItem> itemsToFlush;
                lock (readItems)
                {
                    itemsToFlush = readItems.ToList();
                }

                if (itemsToFlush.Count == 0)
                {
                    return;
                }

                await repository.MarkAsReadAsync(itemsToFlush.Select(i => i.Key));

                lock (readItems)
                {
                    // 書き込んだ分だけ削除する
                    foreach (var item in itemsToFlush)
                    {
                        readItems.Remove(item);
                    }
                }
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