using System;
using System.IO;
using System.Windows;
using Prism.Ioc;
using WebFeedReader.Dbs;
using WebFeedReader.Utils;
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
    }

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var baseDir = AppContext.BaseDirectory;
        var dbPath = Path.Combine(baseDir, "Feeds.db");
        var context = new AppDbContext(dbPath);
        DatabaseInitializer.EnsureDatabase(context);
    }
}