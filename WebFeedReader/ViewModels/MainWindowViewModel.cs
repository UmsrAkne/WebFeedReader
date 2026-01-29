using Prism.Mvvm;
using WebFeedReader.Utils;

namespace WebFeedReader.ViewModels;

public class MainWindowViewModel : BindableBase
{
    private readonly AppVersionInfo appVersionInfo = new ();

    public string Title => appVersionInfo.Title;
}