using CommunityToolkit.Mvvm.Input;

namespace TypeMaster.ViewModel;

public partial class HomeViewModel : BaseViewModel
{
    INavigationService _navigation;
    public HomeViewModel(INavigationService navigation)
    {
        _navigation = navigation;
    }
    [RelayCommand]
    private void NavigateToTypeTest()
    {
        _navigation.NavigateTo<TypeTestViewModel>();
    }

    [RelayCommand]
    private void NavigateToScoreboard()
    {
        _navigation.NavigateTo<ScoreboardViewModel>();
    }

    [RelayCommand]
    private void Quit()
    {
        Application.Current.Shutdown();
    }
}
