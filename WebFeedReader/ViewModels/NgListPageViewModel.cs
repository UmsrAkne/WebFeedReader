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

        public NgListPageViewModel(NgWordService ngWordService)
        {
            this.ngWordService = ngWordService;
        }

        public ObservableCollection<NgWord> NgWords { get; set; } = new ();

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