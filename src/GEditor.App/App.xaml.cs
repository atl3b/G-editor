using System.Windows;
using GEditor.App.Services;
using GEditor.App.ViewModels;
using GEditor.Core.IO;
using GEditor.Core.Management;
using GEditor.Core.Search;
using GEditor.Syntax;
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
        services.AddSingleton<ISearchService, SearchService>();

        // Syntax 层服务 - 注册语法高亮器
        services.AddSingleton<ISyntaxHighlighterRegistry>(sp =>
        {
            var registry = new SyntaxHighlighterRegistry();
            registry.Register(new PlainTextHighlighter());
            registry.Register(new CSharpSyntaxHighlighter());
            registry.Register(new JsonSyntaxHighlighter());
            registry.Register(new XmlSyntaxHighlighter());
            return registry;
        });

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
