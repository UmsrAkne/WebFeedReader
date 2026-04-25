using Prism.Mvvm;

namespace WebFeedReader.Models
{
    public class PaginationStatus : BindableBase
    {
        private int pageSize = 200;
        private int currentOffset;
        private bool isLoading;
        private bool hasMoreItems = true;
        private FeedSource currentSource;

        public int PageSize { get => pageSize; set => SetProperty(ref pageSize, value); }

        public int CurrentOffset { get => currentOffset; set => SetProperty(ref currentOffset, value); }

        public bool IsLoading { get => isLoading; set => SetProperty(ref isLoading, value); }

        public bool HasMoreItems { get => hasMoreItems; set => SetProperty(ref hasMoreItems, value); }

        public FeedSource CurrentSource { get => currentSource; set => SetProperty(ref currentSource, value); }
    }
}