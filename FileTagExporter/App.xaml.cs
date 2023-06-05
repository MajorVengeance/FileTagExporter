using System;
using System.Windows;
using CommunityToolkit.Mvvm.Messaging;
using FileTagExporter.Helpers;
using FileTagExporter.Messages;
using FileTagExporter.Services;
using FileTagExporter.ViewModels;
using FileTagExporter.Views;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace FileTagExporter;

public partial class App : Application
{
    public new static App Current = (App)Application.Current;
    public IServiceProvider Services => AppHost!.Services;

    public static IHost? AppHost { get; private set; }

    public App()
    {
        AppHost = Host.CreateDefaultBuilder()
            .ConfigureServices((hostContext, services) =>
            {
                services.AddSingleton<MainWindow>();
                services.AddTransient<MainWindowViewModel>();
                services.AddSingleton<ProcessResultsViewModel>();
                services.AddSingleton<FileSelectorViewModel>();
                services.AddSingleton<IMessenger>(WeakReferenceMessenger.Default);

                services.AddSingleton<Func<Type, ViewModel>>(serviceProvider => viewModelType => (ViewModel)serviceProvider.GetRequiredService(viewModelType));
                services.AddSingleton<INavigationService, NavigationService>();
                services.AddSingleton<ITagConversionService, TagConversionService>();
            })
            .Build();
        DispatcherHelper.Initialize();
    }

    protected override async void OnStartup(StartupEventArgs e)
    {
        await AppHost!.StartAsync();
        var mainWindow = AppHost.Services.GetRequiredService<MainWindow>();
        mainWindow.Show();

        base.OnStartup(e);
    }

    protected override async void OnExit(ExitEventArgs e)
    {
        await AppHost!.StopAsync();
        base.OnExit(e);
    }
}
