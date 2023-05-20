using CommunityToolkit.Mvvm.Input;

namespace TypeMaster.ViewModel;

public partial class HomeViewModel : BaseViewModel
{
    INavigationService Navigation { get; }
    public HomeViewModel(INavigationService navigation)
    {
        Navigation = navigation;
    }
    [RelayCommand]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "It is used to generate RelayCommand")]
    private void NavigateToTypeTest()
    {
        Navigation.TryNavigateTo<SearchArticlesViewModel>();
    }

    [RelayCommand]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "It is used to generate RelayCommand")]
    private void NavigateToScoreboard()
    {
        Navigation.TryNavigateTo<ScoreboardViewModel>();
    }

    [RelayCommand]
    private void Quit()
    {
        Application.Current.Shutdown();
    }
}
