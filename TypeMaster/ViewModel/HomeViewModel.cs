using CommunityToolkit.Mvvm.Input;

namespace TypeMaster.ViewModel;

public partial class HomeViewModel : BaseViewModel
{
    INavigationService _navigation;
    WikipediaService _wikipediaService;
    public HomeViewModel(INavigationService navigation, WikipediaService wikipediaService)
    {
        _navigation = navigation;
        _wikipediaService = wikipediaService;
    }
    [RelayCommand]
    private void NavigateToTypeTest()
    {
        _wikipediaService.getPageInfoArgs = new RandomPageInfoArgs(200, "en");
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
