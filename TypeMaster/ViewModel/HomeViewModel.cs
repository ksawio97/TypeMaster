using CommunityToolkit.Mvvm.Input;

namespace TypeMaster.ViewModel;

public partial class HomeViewModel : BaseViewModel
{
    [ObservableProperty]
    INavigationService _navigation;
    public HomeViewModel(INavigationService navigation)
    {
       Navigation = navigation;
    }
    [RelayCommand]
    public void ChangePage()
    {
        Navigation.NavigateTo<TypeTestViewModel>();
    }
}
