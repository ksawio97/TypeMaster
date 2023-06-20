﻿using CommunityToolkit.Mvvm.Input;
using System.Diagnostics;
using System.Threading.Tasks;

namespace TypeMaster.ViewModel;

partial class ChooseTextLengthViewModel : AsyncViewModel
{
    [ObservableProperty]
    ButtonBindableArgs[] _buttonBindableArgs;

    [ObservableProperty]
    string? _wikiTitle;

    readonly INavigationService _navigationService;
    readonly CurrentPageService _currentPageService;

    public ChooseTextLengthViewModel(INavigationService navigationService, CurrentPageService currentPageService, SettingsService settingsService)
    {
        _navigationService = navigationService;
        _currentPageService = currentPageService;

        ButtonBindableArgs = new ButtonBindableArgs[]
        {
            new ButtonBindableArgs { IsEnabled = false, RepresentedLength = TextLength.Short },
            new ButtonBindableArgs { IsEnabled = false, RepresentedLength = TextLength.Medium },
            new ButtonBindableArgs { IsEnabled = false, RepresentedLength = TextLength.Long }
        };

        SetUIItemsText(this, new OnLanguageChangedEventArgs(settingsService.GetUITextValue));
    }

    [RelayCommand]
    public async Task LoadDataAsync()
    {
        if (IsBusy) return;
        IsBusy = true;

        string? content = await _currentPageService.TryGetPageContent(formatted: true);
        if (content != null)
        {
            WikiTitle = (await _currentPageService.TryGetPageResult())?.Title;
            CheckIfCanBeEnabled(content.Length);
        }
        else
        {
            Debug.WriteLine("Couldn't get page content!");
        }

        IsBusy = false;
    }

    [RelayCommand]
    void NavigateToTypeTest(TextLength textLength)
    {
        _currentPageService.CurrentPageInfoArgs!.ProvidedTextLength = textLength;
        _navigationService.TryNavigateTo<TypeTestViewModel>();
    }

    void CheckIfCanBeEnabled(int length)
    {
        foreach (var item in ButtonBindableArgs)
            item.IsEnabled = (int)item.RepresentedLength <= length;
    }

    public override void SetUIItemsText(object? _, OnLanguageChangedEventArgs e)
    {
        for(int i = 0; i < ButtonBindableArgs.Length; i++)
            ButtonBindableArgs[i].Content = e.GetText($"TextLength{i}");
    }
}

public partial class ButtonBindableArgs : ObservableObject
{
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Visibility))]
    bool isEnabled;

    [ObservableProperty]
    TextLength representedLength;

    [ObservableProperty]
    string? content;
    public Visibility Visibility => IsEnabled ? Visibility.Visible : Visibility.Hidden;
}