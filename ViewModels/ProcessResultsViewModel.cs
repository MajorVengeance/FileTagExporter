using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Documents;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using FileTagExporter.Events;
using FileTagExporter.Messages;
using FileTagExporter.Models;
using FileTagExporter.Services;

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
