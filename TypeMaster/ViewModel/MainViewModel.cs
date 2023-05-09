using CommunityToolkit.Mvvm.Input;

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

    [RelayCommand]
    private void GoHome()
    {
        if (Navigation.CurrentView is not HomeViewModel)
            Navigation.NavigateTo<HomeViewModel>();
    }
}
