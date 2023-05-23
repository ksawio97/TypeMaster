using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Shapes;

namespace TypeMaster.ViewModel;

public partial class MainViewModel : AsyncViewModel
{
    [ObservableProperty]
    string? _title;

    [ObservableProperty]
    INavigationService _navigation;

    [ObservableProperty]
    List<MenuItem> _languageOptions;

    readonly LanguagesService LanguagesService;
    readonly SettingsService SettingsService;
    readonly CurrentPageService CurrentPageService;
    readonly ColorsService ColorsService;

    public MainViewModel(INavigationService navigation, LanguagesService languagesService, SettingsService settingsService, CurrentPageService currentPageService, ColorsService colorsService)
    {
        LanguagesService = languagesService;
        SettingsService = settingsService;
        CurrentPageService = currentPageService;
        Navigation = navigation;
        ColorsService = colorsService;

        Title = "TypeMaster";
        LanguageOptions = new ();
        SetContextMenuItems();
        navigation.TryNavigateTo<SearchArticlesViewModel>();
    }

    private void SetContextMenuItems()
    {
        foreach (string option in LanguagesService.AvailableLanguages)
            LanguageOptions.Add(CreateOption(option, SettingsService.CurrentLanguage == option));
    }

    private MenuItem CreateOption(string option, bool isSelected)
    {
        SolidColorBrush ColorsOptions(bool selected) => selected ? ColorsService.TryGetColor("DarkBackgroundColor") ?? Brushes.MediumPurple : ColorsService.TryGetColor("BackgroundColor") ?? Brushes.Purple;
        return new MenuItem
        {
            Header = option,
            Command = new RelayCommand(() => OptionChoosed(option, ColorsOptions)),
            HorizontalContentAlignment = HorizontalAlignment.Center,
            VerticalContentAlignment = VerticalAlignment.Center,
            IsEnabled = !isSelected,
            Background = ColorsOptions(isSelected),
            Foreground = ColorsService.TryGetColor("ForegroundColor") ?? Brushes.White,
            Icon = new Path
            {
                Data = Geometry.Parse("M -2,-2 L 2,-2 L 2,2 L -2,2 Z"),
                Fill = ColorsService.TryGetColor("ForegroundColor") ?? Brushes.White,
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

        SettingsService.TryChangeCurrentLanguage(selectedOption);
        OnPropertyChanged(nameof(LanguageOptions));
    }

    
    [RelayCommand]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "It is used to generate RelayCommand")]
    private void NavigateToTypeTest()
    {
        Navigation.TryNavigateTo<SearchArticlesViewModel>();
    }

    [RelayCommand]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "It is used to generate RelayCommand")]
    private async Task NavigateToRandomTypeTest()
    {
        if (IsBusy) return;
        IsBusy = true;

        var pageInfoArgs = await DraftRandomPage();
        
        if (Navigation.CurrentView is not ChooseTextLengthViewModel)
            Navigation.TryNavigateWithPageInfoArgs<ChooseTextLengthViewModel>(pageInfoArgs);
        else
        {
            //if it's ChooseTextLengthViewModel just draft new random page
            var chooseTextLengthViewModel = (ChooseTextLengthViewModel)Navigation.CurrentView;
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
            pageInfoArgs = new RandomPageInfoArgs(null, SettingsService.CurrentLanguage);
            CurrentPageService.CurrentPageInfoArgs = pageInfoArgs;

            (wikipediaPageInfo, content) = (await CurrentPageService.TryGetPageResult(), await CurrentPageService.TryGetPageContent());

            if (wikipediaPageInfo == null || content == null)
                continue;

        } while (content!.Length < (int)TextLength.Short);

        return new IdPageInfoArgs(wikipediaPageInfo!.Id, null, SettingsService.CurrentLanguage);
    }

    [RelayCommand]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "It is used to generate RelayCommand")]
    private void NavigateToScoreboard()
    {
        Navigation.TryNavigateTo<ScoreboardViewModel>();
    }

    [RelayCommand]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "It is used to generate RelayCommand")]
    private void Quit()
    {
        Application.Current.Shutdown();
    }
}