using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using FileTagExporter.Messages;
using FileTagExporter.Models;
using Microsoft.WindowsAPICodePack.Dialogs;

namespace FileTagExporter.ViewModels;

public partial class FileSelectorViewModel : ViewModel
{

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(PathHasValue))]
    private string? _path;

    [ObservableProperty]
    private bool _recursive;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(OpenFileDialogCommand))]
    [NotifyPropertyChangedFor(nameof(IsDirectory))]
    private FileType _selectFolder;

    [ObservableProperty]
    private OverwriteBehavior _overwriteBehavior;

    public FileSelectorViewModel()
    {
        Init();

        OpenFileDialogCommand = new RelayCommand(OpenFileDialog, OpenFileDialogCommandCanExecute);
        StartProcessCommand = new RelayCommand(StartProcess);

        WeakReferenceMessenger.Default.Register<FileSelectorViewModel, FileDetailsRequestMessage>(this, (r, m) =>
        {
            m.Reply(new FileDetails(Path, SelectFolder, OverwriteBehavior, Recursive));
        });
    }

    public bool PathHasValue => !string.IsNullOrWhiteSpace(Path);

    public bool IsDirectory => SelectFolder == FileType.Directory;

    public IRelayCommand OpenFileDialogCommand { get; }

    public IRelayCommand StartProcessCommand { get; }

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

    private void StartProcess()
    {
        WeakReferenceMessenger.Default.Send(new NavigationRequestMessage(typeof(ProcessResultsViewModel)));
        WeakReferenceMessenger.Default.Send(new LongProcessStatusChangeMessage(new(true, "Loading")));
    }

    private void Init()
    {
        SelectFolder = FileType.None;
        OverwriteBehavior = OverwriteBehavior.Ignore;
        Path = string.Empty;
        Recursive = false;
    }

    partial void OnSelectFolderChanged(FileType oldValue, FileType newValue)
    {
        if (newValue != FileType.Directory)
            Recursive = false;
    }

    protected override void NavigationComplete()
    {
        Init();
    }
}
