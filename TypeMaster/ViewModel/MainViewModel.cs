using CommunityToolkit.Mvvm.ComponentModel;

namespace TypeMaster.ViewModel;

public partial class MainViewModel : BaseViewModel
{
    [ObservableProperty]
    string? _title;

    [ObservableProperty]
    Page _windowContent;

    public MainViewModel()
    {
        Title = "TypeMaster";
        WindowContent = new HomeView();
    }
}
