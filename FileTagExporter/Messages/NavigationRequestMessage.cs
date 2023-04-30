using System;
using CommunityToolkit.Mvvm.Messaging.Messages;

namespace FileTagExporter.Messages;

public class NavigationRequestMessage : ValueChangedMessage<Type>
{
    public NavigationRequestMessage(Type value) : base(value) { }
}
