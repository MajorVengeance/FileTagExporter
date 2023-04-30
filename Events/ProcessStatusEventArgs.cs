using System;

namespace FileTagExporter.Events;

public class ProcessStatusEventArgs : EventArgs
{
    public bool IsComplete { get; set; } = false;

    public string? StatusMessage { get; set; }
}
