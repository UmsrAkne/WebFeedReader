using Prism.Mvvm;
using WebFeedReader.Dbs;

namespace WebFeedReader.ViewModels
{
    public class NgListPageViewModel : BindableBase
    {
        private readonly NgWordService ngWordService;

        public NgListPageViewModel(NgWordService ngWordService)
        {
            this.ngWordService = ngWordService;
        }
    }
}