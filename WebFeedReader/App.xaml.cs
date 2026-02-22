using System;
using System.IO;
using System.Windows;
using System.Windows.Threading;
using Prism.Ioc;
using Serilog;
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
    private MainWindowViewModel mainWindowVm;

    protected override Window CreateShell()
    {
        return Container.Resolve<MainWindow>();
    }

    protected override void InitializeShell(Window shell)
    {
        // 起動時に DataContext (ViewModel) を変数に入れておく
        mainWindowVm = shell.DataContext as MainWindowViewModel;
        base.InitializeShell(shell);
    }

    protected override void RegisterTypes(IContainerRegistry containerRegistry)
    {
        // Register AppSettings as a singleton loaded from a configuration file
        var appSettings = AppSettings.Load();
        containerRegistry.RegisterInstance(appSettings);

        // DB パスをここで確定させる
        var baseDir = AppContext.BaseDirectory;
        var dbPath = Path.Combine(baseDir, "Feeds.db");

        containerRegistry.Register<AppDbContext>(() => new AppDbContext(dbPath));

        containerRegistry.Register<Func<AppDbContext>>(() => () => new AppDbContext(dbPath));
        containerRegistry.RegisterSingleton<NgWordService>();

        containerRegistry.RegisterSingleton<IFeedSourceRepository, FeedSourceRepository>();
        containerRegistry.RegisterSingleton<IFeedSourceSyncService, FeedSourceSyncService>();

        containerRegistry.RegisterSingleton<IFeedItemRepository, FeedItemRepository>();
        containerRegistry.RegisterSingleton<IFeedSyncService, FeedSyncService>();

        containerRegistry.Register<FeedListViewModel>();
        containerRegistry.Register<SettingPageViewModel>();

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
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.Console()
            .WriteTo.File(
                path: Path.Combine(baseDir, "logs", "app-.log"),
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 7)
            .CreateLogger();

        var context = Container.Resolve<AppDbContext>();
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

    protected override void OnExit(ExitEventArgs e)
    {
        try
        {
            // Flush pending read items synchronously to ensure they are saved before the app closes
            mainWindowVm.FeedListViewModel.FlushReadItemsAsync().GetAwaiter().GetResult();
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error while flushing read items on application exit");
        }
        finally
        {
            base.OnExit(e);
        }
    }
}