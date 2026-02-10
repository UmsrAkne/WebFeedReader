using Prism.Mvvm;
using WebFeedReader.Utils;

namespace WebFeedReader.ViewModels
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class SettingPageViewModel : BindableBase
    {
        private readonly AppSettings appSettings;

        public SettingPageViewModel(AppSettings appSettings)
        {
            this.appSettings = appSettings;
        }
    }
}