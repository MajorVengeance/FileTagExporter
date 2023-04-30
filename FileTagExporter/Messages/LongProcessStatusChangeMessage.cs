using CommunityToolkit.Mvvm.Messaging.Messages;
using FileTagExporter.Models;

namespace FileTagExporter.Messages;

public class LongProcessStatusChangeMessage : ValueChangedMessage<LongProcessStatus>
{
    public LongProcessStatusChangeMessage(LongProcessStatus value) : base(value)
    {
    }
}
