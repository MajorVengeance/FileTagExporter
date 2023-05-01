using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using FileTagExporter.Messages;
using FileTagExporter.Models;
using Microsoft.WindowsAPICodePack.Dialogs;

namespace FileTagExporter.ViewModels;

public partial class FileSelectorViewModel : ViewModel
{
    public FileSelectorViewModel()
    {
        Init();

        OpenFileDialogCommand = new RelayCommand(OpenFileDialog, OpenFileDialogCommandCanExecute);
        StartProcessCommand = new RelayCommand(StartProcess);

        WeakReferenceMessenger.Default.Register<FileSelectorViewModel, FileDetailsRequestMessage>(this, (r, m) =>
        {
            m.Reply(new FileDetails(Path, SelectFolder, OverwriteBehavior));
        });
    }

    private void Init()
    {
        SelectFolder = FileType.None;
        OverwriteBehavior = OverwriteBehavior.Ignore;
        Path = string.Empty;
    }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(PathHasValue))]
    private string? _path;

    public bool PathHasValue => !string.IsNullOrWhiteSpace(Path);

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(OpenFileDialogCommand))]
    private FileType _selectFolder;

    [ObservableProperty]
    private OverwriteBehavior _overwriteBehavior;

    public IRelayCommand OpenFileDialogCommand { get; }

    public bool OpenFileDialogCommandCanExecute() => SelectFolder != FileType.None;

    private void OpenFileDialog()
    {
        var dialog = new CommonOpenFileDialog
        {
            IsFolderPicker = SelectFolder == FileType.Directory
        };

        CommonFileDialogResult result = dialog.ShowDialog();
        if (result == CommonFileDialogResult.Ok)
        {
            Path = dialog.FileName;
        }
    }

    public IRelayCommand StartProcessCommand { get; }

    private void StartProcess()
    {
        WeakReferenceMessenger.Default.Send(new NavigationRequestMessage(typeof(ProcessResultsViewModel)));
        WeakReferenceMessenger.Default.Send(new LongProcessStatusChangeMessage(new(true, "Loading")));
    }
}
