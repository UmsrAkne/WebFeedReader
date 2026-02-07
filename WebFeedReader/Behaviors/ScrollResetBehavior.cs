using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Microsoft.Xaml.Behaviors;
using WebFeedReader.ViewModels;

namespace WebFeedReader.Behaviors
{
    public sealed class ScrollResetBehavior : Behavior<ListBox>
    {
        private ScrollViewer scrollViewer;
        private bool isResetting;

        protected override void OnAttached()
        {
            base.OnAttached();

            AssociatedObject.Loaded += OnLoaded;

            // 後からの変更を監視
            AssociatedObject.DataContextChanged += OnDataContextChanged;

            if (AssociatedObject.DataContext is IScrollResettable vm)
            {
                vm.RequestScrollReset += OnRequestScrollReset;
            }
        }

        protected override void OnDetaching()
        {
            AssociatedObject.Loaded -= OnLoaded;
            AssociatedObject.DataContextChanged -= OnDataContextChanged;

            Unsubscribe();

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
        }

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            Unsubscribe();

            if (e.NewValue is IScrollResettable vm)
            {
                vm.RequestScrollReset += OnRequestScrollReset;
            }
        }

        private void Unsubscribe()
        {
            if (AssociatedObject.DataContext is IScrollResettable vm)
            {
                vm.RequestScrollReset -= OnRequestScrollReset;
            }
        }

        private void OnRequestScrollReset()
        {
            if (scrollViewer == null)
            {
                return;
            }

            isResetting = true;

            // Dispatcher 経由で UI 更新後に実行
            AssociatedObject.Dispatcher.InvokeAsync(() =>
            {
                scrollViewer.ScrollToVerticalOffset(0);
                isResetting = false;
            });
        }
    }
}