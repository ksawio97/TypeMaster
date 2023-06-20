namespace TypeMaster.ViewModel;

abstract public partial class BaseViewModel : ObservableObject
{
    public abstract void SetUIItemsText(object? sender, OnLanguageChangedEventArgs e);
}
