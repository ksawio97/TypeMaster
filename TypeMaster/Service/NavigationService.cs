using System;

namespace TypeMaster.Service;

public interface INavigationService
{
    BaseViewModel CurrentView { get; }
    void NavigateTo<TViewModel>() where TViewModel : BaseViewModel;
}

public class NavigationService : ObservableObject, INavigationService
{
    private readonly Func<Type, BaseViewModel> _viewModelFactory;

    private BaseViewModel _currentView;

    public BaseViewModel CurrentView
    {
        get => _currentView;
        private set => SetProperty(ref _currentView, value);
    }

    public NavigationService(Func<Type, BaseViewModel> viewModelFactory)
    {
        _viewModelFactory = viewModelFactory;
    }
    public void NavigateTo<TViewModel>() where TViewModel : BaseViewModel
    {
        var viewmodel = _viewModelFactory.Invoke(typeof(TViewModel));
        CurrentView = viewmodel;
    }
}
