using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Microsoft.Xaml.Behaviors;
using WebFeedReader.ViewModels;

namespace WebFeedReader.Behaviors
{
    public sealed class InfiniteScrollBehavior : Behavior<ListBox>
    {
        public readonly static DependencyProperty ThresholdProperty =
            DependencyProperty.Register(
                nameof(Threshold),
                typeof(double),
                typeof(InfiniteScrollBehavior),
                new PropertyMetadata(50d));

        private ScrollViewer scrollViewer;

        /// <summary>
        /// 下端から何px手前でロードを開始するか
        /// </summary>
        public double Threshold
        {
            get => (double)GetValue(ThresholdProperty);
            set => SetValue(ThresholdProperty, value);
        }

        protected override void OnAttached()
        {
            base.OnAttached();

            AssociatedObject.Loaded += OnLoaded;
        }

        protected override void OnDetaching()
        {
            AssociatedObject.Loaded -= OnLoaded;

            if (scrollViewer != null)
            {
                scrollViewer.ScrollChanged -= OnScrollChanged;
            }

            base.OnDetaching();
        }

        private static ScrollViewer FindScrollViewer(DependencyObject root)
        {
            if (root is ScrollViewer sv)
            {
                return sv;
            }

            for (var i = 0; i < VisualTreeHelper.GetChildrenCount(root); i++)
            {
                var child = VisualTreeHelper.GetChild(root, i);
                var result = FindScrollViewer(child);
                if (result != null)
                {
                    return result;
                }
            }

            return null;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            scrollViewer = FindScrollViewer(AssociatedObject);
            if (scrollViewer != null)
            {
                scrollViewer.ScrollChanged += OnScrollChanged;
            }
        }

        // イベントハンドラであるため、void 必須。
        // ReSharper disable once AsyncVoidEventHandlerMethod
        private async void OnScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (scrollViewer == null)
            {
                return;
            }

            if (scrollViewer.VerticalOffset + scrollViewer.ViewportHeight
                < scrollViewer.ExtentHeight - Threshold)
            {
                return;
            }

            if (AssociatedObject.DataContext is not MainWindowViewModel mainVm)
            {
                return;
            }

            var feedListVm = mainVm.FeedListViewModel;
            var currentSource = mainVm.FeedSourceListViewModel.SelectedItem;

            if (feedListVm == null || currentSource == null)
            {
                return;
            }

            await feedListVm.LoadNextPageAsync(currentSource);
        }
    }
}