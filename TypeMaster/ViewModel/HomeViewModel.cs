using CommunityToolkit.Mvvm.Input;

namespace TypeMaster.ViewModel;

public partial class HomeViewModel : BaseViewModel
{
    INavigationService _navigation { get; }
    public HomeViewModel(INavigationService navigation)
    {
        _navigation = navigation;
    }
    [RelayCommand]
    private void NavigateToTypeTest()
    {
        var pageInfoArgs = new RandomPageInfoArgs(200, "en");
        _navigation.NavigateToTypeTestWithPageInfoArgs(pageInfoArgs);
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
