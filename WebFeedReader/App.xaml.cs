using System;
using System.IO;
using System.Windows;
using System.Windows.Threading;
using Prism.Ioc;
using WebFeedReader.Api;
using WebFeedReader.Dbs;
using WebFeedReader.Utils;
using WebFeedReader.ViewModels;
using WebFeedReader.Views;

namespace WebFeedReader;

/// <summary>
///     Interaction logic for App.xaml
/// </summary>
public partial class App
{
    protected override Window CreateShell()
    {
        return Container.Resolve<MainWindow>();
    }

    protected override void RegisterTypes(IContainerRegistry containerRegistry)
    {
        // Register AppSettings as a singleton loaded from a configuration file
        var appSettings = AppSettings.Load();
        containerRegistry.RegisterInstance(appSettings);

        #if DEBUG
        containerRegistry.Register<IApiClient, DummyApiClient>();

        #else
        containerRegistry.Register<IApiClient, ApiClient>();

        #endif
    }

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var baseDir = AppContext.BaseDirectory;
        var dbPath = Path.Combine(baseDir, "Feeds.db");
        var context = new AppDbContext(dbPath);
        DatabaseInitializer.EnsureDatabase(context);
    }

    protected override void OnInitialized()
    {
        base.OnInitialized();

        if (Current.MainWindow?.DataContext is MainWindowViewModel vm)
        {
            // Initialize a view model asynchronously on the UI dispatcher after shell is ready
            Dispatcher.BeginInvoke(async () => await vm.InitializeAsync(), DispatcherPriority.Background);
        }
    }
}