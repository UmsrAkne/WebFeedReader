using Prism.Mvvm;

namespace WebFeedReader.ViewModels
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class SyncStateIndicatorVm : BindableBase
    {
        private bool isFlushing;
        private bool hasError;
        private string lastFlushTime;
        private string statusMessage;

        public bool IsFlushing { get => isFlushing; set => SetProperty(ref isFlushing, value); }

        public bool HasError { get => hasError; set => SetProperty(ref hasError, value); }

        public string LastFlushTime { get => lastFlushTime; set => SetProperty(ref lastFlushTime, value); }

        public string StatusMessage { get => statusMessage; set => SetProperty(ref statusMessage, value); }
    }
}