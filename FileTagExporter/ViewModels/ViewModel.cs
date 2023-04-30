using System;
using CommunityToolkit.Mvvm.ComponentModel;

namespace FileTagExporter.ViewModels;

public abstract class ViewModel : ObservableRecipient
{
    public virtual void NavigationComplete() { }
}
