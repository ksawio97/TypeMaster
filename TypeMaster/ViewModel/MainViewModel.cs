using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using System.Windows.Shapes;

namespace TypeMaster.ViewModel;

public partial class MainViewModel : AsyncViewModel
{
    [ObservableProperty]
    string? _title;

    [ObservableProperty]
    INavigationService _navigationService;

    [ObservableProperty]
    List<MenuItem> _languageOptions;

    readonly LanguagesService _languagesService;
    readonly SettingsService _settingsService;
    readonly CurrentPageService _currentPageService;
    readonly ColorsService _colorsService;

    public MainViewModel(INavigationService navigation, LanguagesService languagesService, SettingsService settingsService, CurrentPageService currentPageService, ColorsService colorsService)
    {
        _languagesService = languagesService;
        _settingsService = settingsService;
        _currentPageService = currentPageService;
        _navigationService = navigation;
        _colorsService = colorsService;

        Title = "TypeMaster";
        LanguageOptions = new ();
        SetContextMenuItems();
        navigation.TryNavigateTo<SearchArticlesViewModel>();
    }

    private void SetContextMenuItems()
    {
        foreach (string option in _languagesService.AvailableLanguages)
            LanguageOptions.Add(CreateOption(option, _settingsService.CurrentLanguage == option));
    }

    private MenuItem CreateOption(string option, bool isSelected)
    {
        SolidColorBrush ColorsOptions(bool selected) => selected ? _colorsService.TryGetColor("DarkBackgroundColor") ?? Brushes.MediumPurple : _colorsService.TryGetColor("BackgroundColor") ?? Brushes.Purple;
        return new MenuItem
        {
            Header = option,
            Command = new RelayCommand(() => OptionChoosed(option, ColorsOptions)),
            HorizontalContentAlignment = HorizontalAlignment.Center,
            VerticalContentAlignment = VerticalAlignment.Center,
            IsEnabled = !isSelected,
            Background = ColorsOptions(isSelected),
            Foreground = _colorsService.TryGetColor("ForegroundColor") ?? Brushes.White,
            Icon = new Path
            {
                Data = Geometry.Parse("M -2,-2 L 2,-2 L 2,2 L -2,2 Z"),
                Fill = _colorsService.TryGetColor("ForegroundColor") ?? Brushes.White,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            }
        };
    }

    private void OptionChoosed(string selectedOption, Func<bool, SolidColorBrush> ColorsOptions)
    {
        foreach (MenuItem option in LanguageOptions)
        {
            bool isSelected = option.Header.Equals(selectedOption);
            option.Background = ColorsOptions(isSelected);
            option.IsEnabled = !isSelected;
        }

        _settingsService.TryChangeCurrentLanguage(selectedOption);
        OnPropertyChanged(nameof(LanguageOptions));
    }

    
    [RelayCommand]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "It is used to generate RelayCommand")]
    private void NavigateToTypeTest()
    {
        _navigationService.TryNavigateTo<SearchArticlesViewModel>();
    }

    [RelayCommand]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "It is used to generate RelayCommand")]
    private async Task NavigateToRandomTypeTest()
    {
        if (!NetworkInterface.GetIsNetworkAvailable() || IsBusy) return;
        IsBusy = true;

        var pageInfoArgs = await DraftRandomPage();
        
        if (_navigationService.CurrentView is not ChooseTextLengthViewModel)
            _navigationService.TryNavigateWithPageInfoArgs<ChooseTextLengthViewModel>(pageInfoArgs);
        else
        {
            //if it's ChooseTextLengthViewModel just draft new random page
            var chooseTextLengthViewModel = (ChooseTextLengthViewModel)_navigationService.CurrentView;
            await chooseTextLengthViewModel.LoadDataAsync();
        }
        IsBusy = false;
    }

    async Task<PageInfoArgs> DraftRandomPage()
    {
        PageInfoArgs pageInfoArgs;
        SearchResult? wikipediaPageInfo;
        string? content;
        do
        {
            pageInfoArgs = new RandomPageInfoArgs(null, _settingsService.CurrentLanguage);
            _currentPageService.CurrentPageInfoArgs = pageInfoArgs;

            (wikipediaPageInfo, content) = (await _currentPageService.TryGetPageResult(), await _currentPageService.TryGetPageContent());

            if (wikipediaPageInfo == null || content == null)
                continue;

        } while (content!.Length < (int)TextLength.Short);

        return new IdPageInfoArgs(wikipediaPageInfo!.Id, null, _settingsService.CurrentLanguage);
    }

    [RelayCommand]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "It is used to generate RelayCommand")]
    private void NavigateToScoreboard()
    {
        _navigationService.TryNavigateTo<ScoreboardViewModel>();
    }

    [RelayCommand]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "It is used to generate RelayCommand")]
    private void Quit()
    {
        Application.Current.Shutdown();
    }
}