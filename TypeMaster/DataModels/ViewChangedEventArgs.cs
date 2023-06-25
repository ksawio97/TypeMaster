using System;

namespace TypeMaster.DataModels;

public class ViewChangedEventArgs : EventArgs
{
    public readonly BaseViewModel? OldViewModel;
    public readonly BaseViewModel NewViewModel;

    public ViewChangedEventArgs(BaseViewModel? oldViewModel, BaseViewModel newViewModel) => (OldViewModel, NewViewModel) = (oldViewModel, newViewModel);
}