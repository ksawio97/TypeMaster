using System;

namespace TypeMaster.Service;

public interface INavigationService
{
    BaseViewModel CurrentView { get; }
    bool TryNavigateTo<TViewModel>() where TViewModel : BaseViewModel;

    bool TryNavigateWithPageInfoArgs<TViewModel>(PageInfoArgs pageInfoArgs) where TViewModel : BaseViewModel;
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

    public bool TryNavigateTo<TViewModel>() where TViewModel : BaseViewModel
    {
        if (_currentView is TViewModel || (typeof(TViewModel) == typeof(TypeTestViewModel) && _wikipediaService.GetPageInfoArgs == null))
            return false;
        var viewmodel = _viewModelFactory.Invoke(typeof(TViewModel));
        CurrentView = viewmodel;
        return true;
    }

    public bool TryNavigateWithPageInfoArgs<TViewModel>(PageInfoArgs pageInfoArgs) where TViewModel : BaseViewModel
    {
        _wikipediaService.GetPageInfoArgs = pageInfoArgs;
        return TryNavigateTo<TViewModel>();
    }
}
