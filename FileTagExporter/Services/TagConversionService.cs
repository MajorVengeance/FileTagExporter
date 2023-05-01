using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FileTagExporter.Events;
using FileTagExporter.Models;
using Microsoft.WindowsAPICodePack.Shell;

namespace FileTagExporter.Services;

public interface ITagConversionService
{
    public Task<List<ImageData>> ConvertTagsToSubjectAsync(string path, FileType fileType, OverwriteBehavior overwriteBehavior, bool recursive);

    event EventHandler<ProcessStatusEventArgs> ProcessStatusEvent;
}

internal class TagConversionService : ITagConversionService
{
    public event EventHandler<ProcessStatusEventArgs>? ProcessStatusEvent;

    private const string _fileSubjectSkipping = "File Subject already contains value: Skipping";
    private const string _fileSubjectUpdated = "File Subject updated";
    private const string _fileTagsMissing = "File has no Tags: Skipping";

    public Task<List<ImageData>> ConvertTagsToSubjectAsync(string path, FileType fileType, OverwriteBehavior overwriteBehavior, bool recursive)
    {
        return Task.Run(() => ConvertTagsToSubject(path, fileType, overwriteBehavior, recursive));
    }

    private List<ImageData> ConvertTagsToSubject(string path, FileType fileType, OverwriteBehavior overwriteBehavior, bool recursive)
    {
        try
        {
            var data = new List<ImageData>();

            if (fileType is FileType.Directory)
                data = Directory.GetFiles(path, "*", recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly)
                    .Select(a => new ImageData(a)).ToList();
            else
                data.Add(new ImageData(path));

            OnRaiseProcessStatusEvent(new ProcessStatusEventArgs() { StatusMessage = $"{data.Count} files to process" });
            var count = 0;
            foreach (var file in data)
            {
                using var shellFile = new ShellFile(file.Location!);
                var writer = shellFile.Properties.GetPropertyWriter();
                var subjectValue = shellFile.Properties.System.Subject.Value;
                var tagsValue = string.Join("; ", shellFile.Properties.System.Keywords.Value ?? Array.Empty<string>());
                if (shellFile.Properties.System.Keywords.Value != null && shellFile.Properties.System.Keywords.Value.Any())
                {
                    switch (overwriteBehavior)
                    {
                        case OverwriteBehavior.Ignore:
                            if (string.IsNullOrEmpty(subjectValue))
                            {
                                writer.WriteProperty(shellFile.Properties.System.Subject, tagsValue);
                                file.Status = _fileSubjectUpdated;
                            }
                            else
                                file.Status = _fileSubjectSkipping;
                            break;
                        case OverwriteBehavior.Append:
                            var subject = $"{subjectValue}{(string.IsNullOrEmpty(subjectValue) ? "" : ";")}{tagsValue}";
                            writer.WriteProperty(shellFile.Properties.System.Subject, subject);
                            file.Status = _fileSubjectUpdated;
                            break;
                        case OverwriteBehavior.Overwrite:
                            writer.WriteProperty(shellFile.Properties.System.Subject, tagsValue);
                            file.Status = _fileSubjectUpdated;
                            break;
                    }
                }
                else
                {
                    file.Status = _fileTagsMissing;
                }
                writer.Close();

                count++;
                if (count % 5 == 0)
                {
                    OnRaiseProcessStatusEvent(new ProcessStatusEventArgs() { StatusMessage = $"Processing {count} of {data.Count}" });
                }
            }

            OnRaiseProcessStatusEvent(new ProcessStatusEventArgs() { StatusMessage = "Finished processing files, generating results" });

            foreach (var file in data)
            {
                using var shellFile = new ShellFile(file.Location!);
                file.Subject = shellFile.Properties.System.Subject.Value;
                file.Tags = shellFile.Properties.System.Keywords.Value?.ToList();
            }

            return data;
        }
        finally
        {
            OnRaiseProcessStatusEvent(new ProcessStatusEventArgs() { IsComplete = true, StatusMessage = "Finished" });
        }
    }

    protected virtual void OnRaiseProcessStatusEvent(ProcessStatusEventArgs e)
    {
        ProcessStatusEvent?.Invoke(this, e);
    }
}
