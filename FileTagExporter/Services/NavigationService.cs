using System;
using CommunityToolkit.Mvvm.ComponentModel;
using FileTagExporter.ViewModels;

namespace FileTagExporter.Services;

public interface INavigationService
{
    public ViewModel CurrentView { get; }

    void NavigateTo<T>() where T : ViewModel;
}

// UWP navigation service
public partial class NavigationService : ObservableObject, INavigationService
{
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CurrentView))]
    private ViewModel? _viewModel;
    private readonly Func<Type, ViewModel> _viewModelFactory;

    public ViewModel CurrentView => this.ViewModel!;

    public NavigationService(Func<Type, ViewModel> viewModelFactory)
    {
        _viewModelFactory = viewModelFactory;
    }

    public void NavigateTo<TViewModel>() where TViewModel : ViewModel
    {
        ViewModel viewModel = _viewModelFactory.Invoke(typeof(TViewModel));
        
        this.ViewModel = viewModel;
        this.ViewModel.NavigationComplete();
    }
}
