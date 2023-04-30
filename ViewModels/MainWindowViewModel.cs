using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using FileTagExporter.Messages;
using FileTagExporter.Services;

namespace FileTagExporter.ViewModels;

public partial class MainWindowViewModel : ViewModel
{
    [ObservableProperty]
    private INavigationService _navigation;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string? _statusText;

    public MainWindowViewModel(INavigationService navigationService)
    {
        Navigation = navigationService;
        NavigateHomeCommand = new RelayCommand(NavigateHome);
        NavigateToResultsCommand = new RelayCommand(NavigateToResults);
        WeakReferenceMessenger.Default.Register<LongProcessStatusChangeMessage>(this, LongProcessStatusChangeMessageReceived);
        WeakReferenceMessenger.Default.Register<NavigationRequestMessage>(this, (r, m) =>
        {
            NavigateTo(m.Value);
        });

        NavigateTo<FileSelectorViewModel>();
    }

    

    public RelayCommand NavigateHomeCommand { get; set; }
    public RelayCommand NavigateToResultsCommand { get; set; }

    public void NavigateToResults()
    {
        StatusText = "Threading the needle";
        //NavigationService.NavigateTo<ProcessResultsViewModel>();
    }

    public void NavigateHome()
    {
        Navigation.NavigateTo<FileSelectorViewModel>();
    }

    private void NavigateTo<T>() => NavigateTo(typeof(T));

    public void NavigateTo(Type type)
    {
        if (type == typeof(FileSelectorViewModel))
            Navigation.NavigateTo<FileSelectorViewModel>();
        if (type == typeof(ProcessResultsViewModel))
            Navigation.NavigateTo<ProcessResultsViewModel>();
    }

    public void LongProcessStatusChangeMessageReceived(object recipient, LongProcessStatusChangeMessage message)
    {
        var r = recipient as MainWindowViewModel;

        r.IsLoading = message.Value.IsRunning;
        r.StatusText = message.Value.StatusText;
    }
}
