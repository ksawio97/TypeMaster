namespace TypeMaster.ViewModel;

public partial class MainViewModel : BaseViewModel
{
    [ObservableProperty]
    string? _title;

    [ObservableProperty]
    INavigationService _navigation;

    public MainViewModel(INavigationService navigation)
    {
        Navigation = navigation;
        Navigation.NavigateTo<HomeViewModel>();
        Title = "TypeMaster";
    }
}
