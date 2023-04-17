using CommunityToolkit.Mvvm.ComponentModel;

namespace TypeMaster.ViewModel;

public partial class BaseViewModel : ObservableObject
{
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsNotBusy))]
    protected bool _isBusy;

    public bool IsNotBusy => !_isBusy;

    [ObservableProperty]
    protected string? _title;
}
