﻿using CommunityToolkit.Mvvm.Input;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Shapes;

namespace TypeMaster.ViewModel;

public partial class MainViewModel : AsyncViewModel
{
    [ObservableProperty]
    SolidColorBrush[] _buttonsBackgroundColors;

    [ObservableProperty]
    string?[] _menuButtonsTexts;

    [ObservableProperty]
    INavigationService _navigationService;

    [ObservableProperty]
    List<MenuItem> _languageOptions;

    readonly string[] _brushesKeys;

    readonly LanguagesService _languagesService;
    readonly SettingsService _settingsService;
    readonly CurrentPageService _currentPageService;
    readonly ColorsService _colorsService;
    readonly NetworkAvailabilityService _networkAvailabilityService;

    public MainViewModel(INavigationService navigationService, LanguagesService languagesService, SettingsService settingsService, CurrentPageService currentPageService, ColorsService colorsService, NetworkAvailabilityService networkAvailabilityService)
    {
        IsBusy = true;

        _languagesService = languagesService;
        _settingsService = settingsService;
        _currentPageService = currentPageService;
        _navigationService = navigationService;
        _colorsService = colorsService;
        _networkAvailabilityService = networkAvailabilityService; 

        MenuButtonsTexts = new string?[5];
        SetUIItemsText(this, new OnLanguageChangedEventArgs(_settingsService.GetUITextValue));
        _settingsService.OnLanguageChanged += SetUIItemsText;

        _brushesKeys = new string[2] { "BackgroundColor", "DarkBackgroundColor" };
        ButtonsBackgroundColors = new SolidColorBrush[3];
        _navigationService.ViewChanged += UpdateButtons;

        LanguageOptions = new ();
        SetContextMenuItems();
        _navigationService.TryNavigateTo<SearchArticlesViewModel>();

        IsBusy = false;
    }

    private void UpdateButtons(object? sender, ViewChangedEventArgs e)
    {
        void CheckValues(BaseViewModel viewModel, SolidColorBrush value)
        {
            if (viewModel is SearchArticlesViewModel) ButtonsBackgroundColors[0] = value;
            else if (viewModel is ChooseTextLengthViewModel) ButtonsBackgroundColors[1] = value;
            else if (viewModel is ScoreboardViewModel) ButtonsBackgroundColors[2] = value;
        }
        //special case #1 if it changes from SearchArticlesViewModel to ChooseTextLengthViewModel not from user input or if ChooseTextLengthViewModel goes to TypeTestViewModel dont update buttons
        if ((IsNotBusy && e.NewViewModel is ChooseTextLengthViewModel) || (e.OldViewModel is ChooseTextLengthViewModel && e.NewViewModel is TypeTestViewModel))
            return;
        //special case #2 if it changes from SearchArticlesViewModel to any viewmodel clear all other buttons
        if (e.OldViewModel is TypeTestViewModel)
        {
            if (e.NewViewModel is not SearchArticlesViewModel) ButtonsBackgroundColors[0] = _colorsService.TryGetColor(_brushesKeys[0]) ?? Brushes.MediumPurple;
            if (e.NewViewModel is not ChooseTextLengthViewModel) ButtonsBackgroundColors[1] = _colorsService.TryGetColor(_brushesKeys[0]) ?? Brushes.MediumPurple;
            if (e.NewViewModel is not ScoreboardViewModel) ButtonsBackgroundColors[2] = _colorsService.TryGetColor(_brushesKeys[0]) ?? Brushes.MediumPurple;
        }
        //special case #3 if CurrentView didn't changed but it drafted new page to ChooseTextLengthViewModel
        else if (sender == this)
        {
            ButtonsBackgroundColors[0] = _colorsService.TryGetColor(_brushesKeys[0]) ?? Brushes.MediumPurple;
            ButtonsBackgroundColors[1] = _colorsService.TryGetColor(_brushesKeys[0]) ?? Brushes.Purple;
        }
        else if (e.OldViewModel != null)
            CheckValues(e.OldViewModel, _colorsService.TryGetColor(_brushesKeys[0]) ?? Brushes.MediumPurple);
        CheckValues(e.NewViewModel, _colorsService.TryGetColor(_brushesKeys[1]) ?? Brushes.Purple);
        

        OnPropertyChanged(nameof(ButtonsBackgroundColors));
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
        if (IsBusy || !_networkAvailabilityService.CheckAvailability()) return;
        IsBusy = true;

        var pageInfoArgs = await TryDraftRandomPage();

        if (pageInfoArgs != null)
            if (_navigationService.CurrentView is not ChooseTextLengthViewModel)
                _navigationService.TryNavigateWithPageInfoArgs<ChooseTextLengthViewModel>(pageInfoArgs);
            else
            {
                //if it's ChooseTextLengthViewModel just draft new random page
                var chooseTextLengthViewModel = (ChooseTextLengthViewModel)_navigationService.CurrentView;
                await chooseTextLengthViewModel.LoadDataAsync();
                UpdateButtons(this, new ViewChangedEventArgs(null, chooseTextLengthViewModel));
            }
        else
            _networkAvailabilityService.CheckAvailability();
        IsBusy = false;
    }

    async Task<PageInfoArgs?> TryDraftRandomPage()
    {
        PageInfoArgs pageInfoArgs;
        SearchResult? wikipediaPageInfo;
        string? content;
        do
        {
            pageInfoArgs = new RandomPageInfoArgs(null, _settingsService.CurrentLanguage);
            _currentPageService.CurrentPageInfoArgs = pageInfoArgs;

            (wikipediaPageInfo, content) = (await _currentPageService.TryGetPageResult(), await _currentPageService.TryGetPageContent(formatted: true));

            if (wikipediaPageInfo == null || content == null)
                return null;

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

    public override void SetUIItemsText(object? _, OnLanguageChangedEventArgs e)
    { 
        for (int i = 0; i < MenuButtonsTexts.Length; i++)
            MenuButtonsTexts[i] = e.GetText($"MenuButton{i}");
        OnPropertyChanged(nameof(MenuButtonsTexts));
    }
}