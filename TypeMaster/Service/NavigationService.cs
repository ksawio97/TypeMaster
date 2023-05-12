using System;

namespace TypeMaster.Service;

public interface INavigationService
{
    BaseViewModel CurrentView { get; }
    void NavigateTo<TViewModel>() where TViewModel : BaseViewModel;

    void NavigateToTypeTestWithPageInfoArgs(PageInfoArgs pageInfoArgs);
}

public class NavigationService : ObservableObject, INavigationService
{
    private readonly Func<Type, BaseViewModel> _viewModelFactory;

    private BaseViewModel _currentView;
    private WikipediaService _wikipediaService { get; }

    public BaseViewModel CurrentView
    {
        get => _currentView;
        private set => SetProperty(ref _currentView, value);
    }

    public NavigationService(Func<Type, BaseViewModel> viewModelFactory, WikipediaService wikipediaService)
    {
        _viewModelFactory = viewModelFactory;
        _wikipediaService = wikipediaService;
    }

    public void NavigateTo<TViewModel>() where TViewModel : BaseViewModel
    {
        var viewmodel = _viewModelFactory.Invoke(typeof(TViewModel));
        CurrentView = viewmodel;
    }

    public void NavigateToTypeTestWithPageInfoArgs(PageInfoArgs pageInfoArgs)
    {
        _wikipediaService.getPageInfoArgs = pageInfoArgs;
        NavigateTo<TypeTestViewModel>();
    }
}
