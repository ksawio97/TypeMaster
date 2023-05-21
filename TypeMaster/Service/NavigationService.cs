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
    private readonly Func<Type, BaseViewModel> ViewModelFactory;

    private BaseViewModel _currentView;

    private CurrentPageService CurrentPageService { get; }

    public BaseViewModel CurrentView
    {
        get => _currentView;
        private set => SetProperty(ref _currentView, value);
    }

    public NavigationService(Func<Type, BaseViewModel> viewModelFactory, CurrentPageService currentPageService)
    {
        ViewModelFactory = viewModelFactory;

        CurrentPageService = currentPageService;
    }

    public bool TryNavigateTo<TViewModel>() where TViewModel : BaseViewModel
    {
        if (_currentView is TViewModel || (typeof(TViewModel) == typeof(TypeTestViewModel) && CurrentPageService.IsCurrentPageInfoArgsNull))
            return false;
        var viewmodel = ViewModelFactory.Invoke(typeof(TViewModel));
        CurrentView = viewmodel;
        return true;
    }

    public bool TryNavigateWithPageInfoArgs<TViewModel>(PageInfoArgs pageInfoArgs) where TViewModel : BaseViewModel
    {
        CurrentPageService.CurrentPageInfoArgs = pageInfoArgs;
        return TryNavigateTo<TViewModel>();
    }
}
