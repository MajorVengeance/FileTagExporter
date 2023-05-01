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
    public Task<List<ImageData>> ConvertTagsToSubjectAsync(string path, FileType fileType, OverwriteBehavior overwriteBehavior);

    event EventHandler<ProcessStatusEventArgs> ProcessStatusEvent;
}

internal class TagConversionService : ITagConversionService
{
    public event EventHandler<ProcessStatusEventArgs>? ProcessStatusEvent;

    public Task<List<ImageData>> ConvertTagsToSubjectAsync(string path, FileType fileType, OverwriteBehavior overwriteBehavior)
    {
        return Task.Run(() => ConvertTagsToSubject(path, fileType, overwriteBehavior));
    }

    private List<ImageData> ConvertTagsToSubject(string path, FileType fileType, OverwriteBehavior overwriteBehavior)
    {
        try
        {
            var data = new List<ImageData>();

            if (fileType is FileType.Directory)
                data = Directory.GetFiles(path).Select(a => new ImageData(a)).ToList();
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
                                writer.WriteProperty(shellFile.Properties.System.Subject, tagsValue);
                            else
                                file.Status = "File Subject already contains value: Skipping";
                            break;
                        case OverwriteBehavior.Append:
                            var subject = $"{subjectValue}{(string.IsNullOrEmpty(subjectValue) ? "" : ";")}{tagsValue}";
                            writer.WriteProperty(shellFile.Properties.System.Subject, subject);
                            break;
                        case OverwriteBehavior.Overwrite:
                            writer.WriteProperty(shellFile.Properties.System.Subject, tagsValue);
                            break;
                    }
                }
                else
                {
                    file.Status = "File has no Tags: Skipping";
                }
                writer.Close();

                count++;
                if (count % 5 == 0)
                {
                    OnRaiseProcessStatusEvent(new ProcessStatusEventArgs() { StatusMessage = $"Processing {count} of {data.Count}" });
                }
            }

            OnRaiseProcessStatusEvent(new ProcessStatusEventArgs() { StatusMessage = $"Finished processing files, generating results" });

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
