using CommunityToolkit.Mvvm.Messaging.Messages;
using FileTagExporter.Models;

namespace FileTagExporter.Messages;

internal class FileDetailsMessage : ValueChangedMessage<FileDetails>
{
    public FileDetailsMessage(FileDetails value) : base(value)
    {
    }
}

internal class FileDetailsRequestMessage : AsyncRequestMessage<FileDetails>
{

}
