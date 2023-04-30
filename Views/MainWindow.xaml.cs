using System.Windows;
using FileTagExporter.ViewModels;

namespace FileTagExporter.Views;

public partial class MainWindow : Window
{
    public MainWindow(MainWindowViewModel dataContext)
    {
        InitializeComponent();
        DataContext = dataContext;
    }

    public MainWindowViewModel ViewModel => (MainWindowViewModel)DataContext;
}