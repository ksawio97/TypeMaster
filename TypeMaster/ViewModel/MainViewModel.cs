using CommunityToolkit.Mvvm.Input;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Windows.Data;
using System.Windows.Shapes;

namespace TypeMaster.ViewModel;

public partial class MainViewModel : BaseViewModel
{
    [ObservableProperty]
    string? _title;

    [ObservableProperty]
    INavigationService _navigation;

    [ObservableProperty]
    List<MenuItem> _settingsContextMenuItems;

    LanguagesService LanguagesService { get; }
    SettingsService SettingsService { get; }
    public MainViewModel(INavigationService navigation, LanguagesService languagesService, SettingsService settingsService)
    {
        LanguagesService = languagesService;
        SettingsService = settingsService;

        Navigation = navigation;
        Navigation.TryNavigateTo<HomeViewModel>();

        Title = "TypeMaster";
        AddItemsToContextMenu();
    }

    private void AddItemsToContextMenu()
    {
        //adds list of languages
        var languageMenu = new MenuItem
        {
            Header = "Language",
            HorizontalContentAlignment = HorizontalAlignment.Center,
            VerticalContentAlignment = VerticalAlignment.Center,
            Icon = new Path
            {
                Data = Geometry.Parse("M9 7h6v14h3V7h6V4H9v3z"),
                Fill = Brushes.Black
            }
        };
        //adds options to choose
        foreach (string option in LanguagesService.AvailableLanguages)
            languageMenu.Items.Add(CreateOption(languageMenu, option, SettingsService.CurrentLanguage, SettingsService.TryChangeCurrentLanguage));
        
        SettingsContextMenuItems = new List<MenuItem>
        {
            languageMenu
        };
    }

    private MenuItem CreateOption(MenuItem languageMenu, string option, string selectedHeader, Action<string> setAction)
    {
        return new MenuItem
        {
            Header = option,
            HorizontalContentAlignment = HorizontalAlignment.Center,
            VerticalContentAlignment = VerticalAlignment.Center,
            Command = new RelayCommand(() => OptionChoosed(option, languageMenu.Items, setAction)),
            Background = option == selectedHeader ? Brushes.MediumPurple : Brushes.White,
            Icon = new Path
            {
                Data = Geometry.Parse("M -2,-2 L 2,-2 L 2,2 L -2,2 Z"),
                Fill = Brushes.Black,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            }
        };
    }

    private void OptionChoosed(string selectedOption, ItemCollection options, Action<string> setAction)
    {
        foreach (MenuItem option in options)
        {
            if (!option.Header.Equals(selectedOption))
            {
                if (option.Background != Brushes.White)
                    option.Background = Brushes.White;
            }
            else option.Background = Brushes.MediumPurple;     
        }
        setAction(selectedOption);
    }

    [RelayCommand]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "It is used to generate RelayCommand")]
    private void GoHome()
    {
        Navigation.TryNavigateTo<HomeViewModel>();
    }
}
