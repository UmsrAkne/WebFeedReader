using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.Input;
using Prism.Mvvm;
using WebFeedReader.Dbs;
using WebFeedReader.Models;

namespace WebFeedReader.ViewModels
{
    public class NgListPageViewModel : BindableBase
    {
        private readonly NgWordService ngWordService;
        private string pendingNgWord = string.Empty;
        private int ngWordCount;

        public NgListPageViewModel(NgWordService ngWordService)
        {
            this.ngWordService = ngWordService;
        }

        public ObservableCollection<NgWord> NgWords { get; set; } = new ();

        public string PendingNgWord { get => pendingNgWord; set => SetProperty(ref pendingNgWord, value); }

        public int NgWordCount { get => ngWordCount; set => SetProperty(ref ngWordCount, value); }

        public AsyncRelayCommand LoadNgWordsCommand => new (async () =>
        {
            NgWords.Clear();
            var list = await ngWordService.GetAllNgWordsAsync();
            var l = list.ToList();

            foreach (var ngWord in l)
            {
                ngWord.Value = MaskExceptFirst(ngWord.Value);
            }

            NgWords.AddRange(l);
        });

        public AsyncRelayCommand AddNgWordCommand => new (async () =>
        {
            if (string.IsNullOrWhiteSpace(PendingNgWord))
            {
                return;
            }

            var added = await ngWordService.AddNgWordAsync(
                new NgWord { Value = PendingNgWord, });

            PendingNgWord = string.Empty;

            if (added)
            {
                var l = await ngWordService.GetAllNgWordsAsync();
                NgWordCount = l.Count();
            }
        });

        private string MaskExceptFirst(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return value;
            }

            if (value.Length == 1)
            {
                return value;
            }

            return value[0] + new string('*', value.Length - 1);
        }
    }
}