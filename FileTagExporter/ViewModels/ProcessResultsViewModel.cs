using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using FileTagExporter.Events;
using FileTagExporter.Messages;
using FileTagExporter.Models;
using FileTagExporter.Services;
using Microsoft.WindowsAPICodePack.Dialogs;

namespace FileTagExporter.ViewModels;

public partial class ProcessResultsViewModel : ViewModel
{
    private readonly ITagConversionService _tagConversionService;

    [ObservableProperty]
    private FileDetails? _fileDetails;

    [ObservableProperty]
    private List<ImageData> _processResults = new();

    private Task<List<ImageData>>? _tagConversionTask;

    public ProcessResultsViewModel(ITagConversionService tagConversionService)
    {
        _tagConversionService = tagConversionService;
        _tagConversionService.ProcessStatusEvent += OnProcessStatusEventReceived;

        ExportCsvCommand = new RelayCommand(ExportCsv, ExportCommandCanExecute);
        ExportJsonCommand = new RelayCommand(ExportJson, ExportCommandCanExecute);
        GoBackCommand = new RelayCommand(GoBack);
    }

    public IRelayCommand ExportCsvCommand { get; }

    public IRelayCommand ExportJsonCommand { get; }

    public IRelayCommand GoBackCommand { get; }

    public bool ExportCommandCanExecute() => ProcessResults.Count > 0;


    private void ExportCsv()
    {
        WeakReferenceMessenger.Default.Send(new LongProcessStatusChangeMessage(new(true, "Exporting Data")));
        var exportFilePath = CreateExportFile("csv");
        if(exportFilePath != null)
        {
            var csv = new StringBuilder();
            csv.AppendLine("Location,Subject,Tags,Status");
            ProcessResults.ForEach(r => csv.AppendLine(
                $"{r.Location},{r.Subject},{string.Join("; ", r.Tags ?? new List<string>())},{r.Status}"));
            File.WriteAllText(exportFilePath, csv.ToString());
        }
        WeakReferenceMessenger.Default.Send(new LongProcessStatusChangeMessage(new(false, string.Empty)));
    }

    private void ExportJson()
    {
        WeakReferenceMessenger.Default.Send(new LongProcessStatusChangeMessage(new(true, "Exporting Data")));
        var exportFilePath = CreateExportFile("json");
        if(exportFilePath != null) 
        {
            using JsonDocument document = JsonSerializer.SerializeToDocument(ProcessResults);
            using var stream = new MemoryStream();
            using var jsonWriter = new Utf8JsonWriter(stream, new JsonWriterOptions { Indented = true });
            document.WriteTo(jsonWriter);
            jsonWriter.Flush();
            File.WriteAllText(exportFilePath, System.Text.Encoding.UTF8.GetString(stream.ToArray()));
        }

        WeakReferenceMessenger.Default.Send(new LongProcessStatusChangeMessage(new(false, string.Empty)));
    }

    private static string? CreateExportFile(string extension)
    {
        var dialog = new CommonSaveFileDialog
        {
            AlwaysAppendDefaultExtension = true,
            DefaultFileName = $"Export_{DateTime.Now:yyyyMMdd_HHmmss}",
            DefaultExtension = extension,
        };
        dialog.Filters.Add(new CommonFileDialogFilter(extension, extension));
        CommonFileDialogResult result = dialog.ShowDialog();
        if(result == CommonFileDialogResult.Ok)
        {
            return dialog.FileName;
        }

        return null;
    }

    private void GoBack() 
    {
        WeakReferenceMessenger.Default.Send(new NavigationRequestMessage(typeof(FileSelectorViewModel)));
    }

    private async void OnProcessStatusEventReceived(object? sender, ProcessStatusEventArgs e)
    {
        if (e.IsComplete)
            ProcessResults = await _tagConversionTask!;
        WeakReferenceMessenger.Default.Send(new LongProcessStatusChangeMessage(new(!e.IsComplete, e.StatusMessage)));
    }

    public override async void NavigationComplete()
    {
        FileDetails = await WeakReferenceMessenger.Default.Send<FileDetailsRequestMessage>();
        _tagConversionTask = _tagConversionService.ConvertTagsToSubjectAsync(FileDetails.Path!, FileDetails.FileType, FileDetails.OverwriteBehavior);
        WeakReferenceMessenger.Default.Send(new LongProcessStatusChangeMessage(new(false, string.Empty)));
    }
}
