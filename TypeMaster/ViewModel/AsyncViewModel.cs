namespace TypeMaster.ViewModel;

public partial class AsyncViewModel : BaseViewModel
{
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsNotBusy))]
    [NotifyPropertyChangedFor(nameof(Cursor))]
    protected bool _isBusy;
    public bool IsNotBusy => !IsBusy;

    public Cursor Cursor => IsBusy ? Cursors.Wait : Cursors.Arrow;
}
