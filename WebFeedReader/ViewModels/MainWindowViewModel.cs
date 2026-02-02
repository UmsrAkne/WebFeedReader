using Prism.Mvvm;
using WebFeedReader.Utils;

namespace WebFeedReader.ViewModels;

public class MainWindowViewModel : BindableBase
{
    private readonly AppVersionInfo appVersionInfo = new ();
    private readonly AppSettings appSettings;

    public MainWindowViewModel()
    {
    }

    public MainWindowViewModel(AppSettings appSettings)
    {
        this.appSettings = appSettings;
    }

    public string Title => appVersionInfo.Title;
}