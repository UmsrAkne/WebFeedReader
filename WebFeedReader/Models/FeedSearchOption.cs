using System;
using Prism.Mvvm;

namespace WebFeedReader.Models
{
    public class FeedSearchOption : BindableBase
    {
        private bool isUnreadOnly;
        private bool isReverseOrder;
        private DateTime publishedFrom;
        private DateTime publishedTo;
        private string keyword = string.Empty;

        public bool IsReverseOrder { get => isReverseOrder; set => SetProperty(ref isReverseOrder, value); }

        public bool IsUnreadOnly { get => isUnreadOnly; set => SetProperty(ref isUnreadOnly, value); }

        public DateTime PublishedFrom { get => publishedFrom; set => SetProperty(ref publishedFrom, value); }

        public DateTime PublishedTo { get => publishedTo; set => SetProperty(ref publishedTo, value); }

        public string Keyword { get => keyword; set => SetProperty(ref keyword, value); }
    }
}