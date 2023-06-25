using System;

namespace TypeMaster.Service;

public interface INavigationService
{
    public BaseViewModel CurrentView { get; }
    bool TryNavigateTo<TViewModel>() where TViewModel : BaseViewModel;

    bool TryNavigateWithPageInfoArgs<TViewModel>(PageInfoArgs pageInfoArgs) where TViewModel : BaseViewModel;

    public event EventHandler<ViewChangedEventArgs> ViewChanged;
}

public class NavigationService : ObservableObject, INavigationService
{
    private readonly Func<Type, BaseViewModel> ViewModelFactory;

    private BaseViewModel _currentView;

    readonly CurrentPageService _currentPageService;
    readonly SettingsService _settingsService;

    public event EventHandler<ViewChangedEventArgs> ViewChanged;
    public BaseViewModel CurrentView
    {
        get => _currentView;
        private set => SetProperty(ref _currentView, value);
    }

    public NavigationService(Func<Type, BaseViewModel> viewModelFactory, CurrentPageService currentPageService, SettingsService settingsService)
    {
        ViewModelFactory = viewModelFactory;
        _currentPageService = currentPageService;
        _settingsService = settingsService;

        ViewChanged += SetupTextLanguageTranslation;
    }

    void NavigateTo<TViewModel>() where TViewModel : BaseViewModel
    {
        var viewmodel = CurrentView;
        CurrentView = ViewModelFactory.Invoke(typeof(TViewModel));

        ViewChanged(this, new ViewChangedEventArgs(viewmodel, CurrentView));
    }

    public bool TryNavigateTo<TViewModel>() where TViewModel : BaseViewModel
    {
        if (CurrentView is TViewModel || (typeof(TViewModel) == typeof(TypeTestViewModel) && _currentPageService.IsCurrentPageInfoArgsNull))
            return false;

        NavigateTo<TViewModel>();
        return true;
    }

    private void SetupTextLanguageTranslation(object? sender, ViewChangedEventArgs e)
    {
        if (e.OldViewModel != null)
            _settingsService.OnLanguageChanged -= e.OldViewModel.SetUIItemsText;
        e.NewViewModel.SetUIItemsText(this, new OnLanguageChangedEventArgs(_settingsService.GetUITextValue));

        _settingsService.OnLanguageChanged += e.NewViewModel.SetUIItemsText;
    }

    public bool TryNavigateWithPageInfoArgs<TViewModel>(PageInfoArgs pageInfoArgs) where TViewModel : BaseViewModel
    {
        _currentPageService.CurrentPageInfoArgs = pageInfoArgs;
        return TryNavigateTo<TViewModel>();
    }
}