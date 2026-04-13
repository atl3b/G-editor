using System.Windows;
using GEditor.App.Services;
using GEditor.App.ViewModels;
using GEditor.Core.IO;
using GEditor.Core.Management;
using Microsoft.Extensions.DependencyInjection;

namespace GEditor.App;

public partial class App : Application
{
    private readonly IServiceProvider _serviceProvider;

    public App()
    {
        var services = new ServiceCollection();
        ConfigureServices(services);
        _serviceProvider = services.BuildServiceProvider();
    }

    private void ConfigureServices(IServiceCollection services)
    {
        // Core 层服务
        services.AddSingleton<IEncodingDetector, EncodingDetector>();
        services.AddSingleton<ILineEndingDetector, LineEndingDetector>();
        services.AddSingleton<ITextFileService, TextFileService>();
        services.AddSingleton<IDocumentManager, DocumentManager>();

        // App 层服务
        services.AddSingleton<IDialogService, WpfDialogService>();

        // ViewModels
        services.AddSingleton<MainWindowViewModel>();

        // Windows
        services.AddTransient<MainWindow>();
    }

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
        mainWindow.Show();
    }
}
