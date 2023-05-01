namespace FileTagExporter.Models;

public record FileDetails(string? Path, FileType FileType, OverwriteBehavior OverwriteBehavior = OverwriteBehavior.Ignore, bool Recursive = false);
