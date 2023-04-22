namespace TypeMaster.ViewModel;

public partial class BaseViewModel : ObservableObject
{
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsNotBusy))]
    protected bool _isBusy;

    public bool IsNotBusy => !IsBusy;
}
