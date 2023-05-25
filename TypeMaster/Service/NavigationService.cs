using System;

namespace TypeMaster.Service;

public interface INavigationService
{
    public BaseViewModel CurrentView { get; }
    bool TryNavigateTo<TViewModel>() where TViewModel : BaseViewModel;

    bool TryNavigateWithPageInfoArgs<TViewModel>(PageInfoArgs pageInfoArgs) where TViewModel : BaseViewModel;
}

public class NavigationService : ObservableObject, INavigationService
{
    private readonly Func<Type, BaseViewModel> ViewModelFactory;

    private BaseViewModel _currentView;

    readonly CurrentPageService _currentPageService;

    public BaseViewModel CurrentView
    {
        get => _currentView;
        private set => SetProperty(ref _currentView, value);
    }

    public NavigationService(Func<Type, BaseViewModel> viewModelFactory, CurrentPageService currentPageService)
    {
        ViewModelFactory = viewModelFactory;
        _currentPageService = currentPageService;
    }

    public bool TryNavigateTo<TViewModel>() where TViewModel : BaseViewModel
    {            
        if (CurrentView is TViewModel || (typeof(TViewModel) == typeof(TypeTestViewModel) && _currentPageService.IsCurrentPageInfoArgsNull))
            return false;
        var viewmodel = ViewModelFactory.Invoke(typeof(TViewModel));
        CurrentView = viewmodel;
        return true;
    }

    public bool TryNavigateWithPageInfoArgs<TViewModel>(PageInfoArgs pageInfoArgs) where TViewModel : BaseViewModel
    {
        _currentPageService.CurrentPageInfoArgs = pageInfoArgs;
        return TryNavigateTo<TViewModel>();
    }
}
