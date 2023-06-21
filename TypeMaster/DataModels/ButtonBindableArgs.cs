namespace TypeMaster.DataModels;

public partial class ButtonBindableArgs : ObservableObject
{
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Visibility))]
    bool isEnabled;

    [ObservableProperty]
    TextLength representedLength;

    [ObservableProperty]
    string? content;
    public Visibility Visibility => IsEnabled ? Visibility.Visible : Visibility.Hidden;
}