﻿using CommunityToolkit.Mvvm.Input;

namespace TypeMaster.ViewModel;

public partial class HomeViewModel : BaseViewModel
{
    INavigationService _navigation;
    public HomeViewModel(INavigationService navigation)
    {
        _navigation = navigation;
    }
    [RelayCommand]
    public void NavigateToTypeTest()
    {
        _navigation.NavigateTo<TypeTestViewModel>();
    }

    [RelayCommand]
    public void NavigateToScoreboard()
    {
        _navigation.NavigateTo<ScoreboardViewModel>();
    }
}
